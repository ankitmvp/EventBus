using System;
namespace EventBus.Abstract
{
    public abstract class Event
    {
        public DateTime Timestamp { get; protected set; }
        public Event()
        {
            Timestamp = DateTime.Now;
        }
    }
}
