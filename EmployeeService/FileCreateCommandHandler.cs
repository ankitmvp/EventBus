using System;
using System.Threading;
using System.Threading.Tasks;
using EventBus.Abstract;
using EventBus.Contract;
using MediatR;

namespace EmployeeService
{
    public class FileCreateCommandHandler : IRequestHandler<FileCreateCommand, bool>
    {
        private readonly IEventBus _bus;

        public FileCreateCommandHandler(IEventBus bus)
        {
            _bus = bus;
        }

        public Task<bool> Handle(FileCreateCommand request, CancellationToken cancellationToken)
        {
            _bus.Publish(new FileCreateEvent(request._data));
            return Task.FromResult(true);
        }
    }

    public class FileCreateCommand : Command
    {
        public string _data { get; private set; }
        public FileCreateCommand(string data)
        {
            _data = data;
        }
    }

    public class FileCreateEvent : Event
    {
        public string _data { get; private set; }
        public FileCreateEvent(string data)
        {
            _data = data;
        }
    }
}
