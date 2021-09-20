using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Abstract;
using EventBus.Contract;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.Concrete
{
    public class RMQBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private static IConnection _connection;
        private static RMQSettings _settings;
        private static ConnectionFactory _connectionFactory;
        private readonly object _locker = new();
        private readonly bool _disposed;
        public static bool Created;
        public double ConnectionTime { get; private set; }

        private IConfiguration _config;

        public RMQBus(IMediator mediator, IServiceScopeFactory serviceScopeFactory, IConfiguration config)
        {
            _mediator = mediator;
            _serviceScopeFactory = serviceScopeFactory;
            _config = config;
            _handlers = new();
            _eventTypes = new();

            if (!Created)
            {
                _settings = new RMQSettings(_config);
                _connectionFactory = CreateConnectionFactory();
                if (_settings.IsSslEnabled)
                {
                    _connectionFactory.Ssl.Enabled = _settings.IsSslEnabled;
                    _connectionFactory.Ssl.ServerName = _settings.SslServerName;
                    _connectionFactory.Ssl.Version = System.Security.Authentication.SslProtocols.Tls12;
                }
                else
                    _connectionFactory.Ssl.Enabled = false;
                TryConnect();
                Created = true;
            }
        }

        public bool IsConnected { get { return _connection != null && _connection.IsOpen; } }

        private void TryConnect()
        {
            lock (_locker)
            {
                if (!IsConnected && !Created)
                {
                    var startTime = DateTime.Now;
                    try
                    {
                        _connection = _connectionFactory.CreateConnection();
                        if (IsConnected)
                        {
                            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
                            _connection.CallbackException += OnCallbackException;
                            _connection.ConnectionBlocked += OnConnectionBlocked;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    ConnectionTime = Math.Round(new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMilliseconds);
                }
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;
            TryConnect();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;
            TryConnect();
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (_disposed) return;
            TryConnect();
        }

        private ConnectionFactory CreateConnectionFactory()
        {
            return new ConnectionFactory
            {
                RequestedHeartbeat = _settings.RequestHeartBeat,
                AutomaticRecoveryEnabled = true,
                Uri = _settings.Uri,
                Port = _settings.Port,
                VirtualHost = _settings.VirtualHost,
                DispatchConsumersAsync = true
            };
        }

        public void Publish<T>(T @event) where T : Event
        {
            try
            {
                using var channel = _connection.CreateModel();
                var eventName = @event.GetType().Name;
                channel.QueueDeclare(eventName, false, false, false, null);
                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish("", eventName, null, body);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            return _mediator.Send(command);
        }

        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            try
            {
                var eventName = typeof(T).Name;
                var handlerType = typeof(TH);

                if (!_eventTypes.Contains(typeof(T))) _eventTypes.Add(typeof(T));
                if (!_handlers.ContainsKey(eventName)) _handlers.Add(eventName, new List<Type>());
                if (_handlers[eventName].Any(s => s.GetType() == handlerType)) throw new ArgumentException($"Handler type {handlerType.Name} already registered for event {eventName}", nameof(handlerType));
                _handlers[eventName].Add(handlerType);
                StartBasicConsume<T>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void StartBasicConsume<T>() where T : Event
        {
            var channel = _connection.CreateModel();
            var eventName = typeof(T).Name;
            channel.QueueDeclare(eventName, false, false, false, null);
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;
            channel.BasicConsume(eventName, true, consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;
            var body = @event.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            try
            {
                await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var subscriptions = _handlers[eventName];
                foreach (var subscription in subscriptions)
                {
                    var handler = scope.ServiceProvider.GetService(subscription);
                    if (handler == null) continue;
                    var eventType = _eventTypes.SingleOrDefault(t => t.Name == eventName);
                    var @event = JsonConvert.DeserializeObject(message, eventType);
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                }
            }
        }
    }
}
