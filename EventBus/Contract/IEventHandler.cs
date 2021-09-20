using System;
using System.Threading.Tasks;
using EventBus.Abstract;

namespace EventBus.Contract
{
    public interface IEventHandler<in TEvent> : IEventHandler where TEvent : Event
    {
        Task Handle(TEvent @event);
    }
    public interface IEventHandler
    {

    }
}
