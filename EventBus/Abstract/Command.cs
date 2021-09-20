using System;
namespace EventBus.Abstract
{
    public abstract class Command : Message
    {
        public DateTime Timestamp { get; protected set; }
        public Command()
        {
            Timestamp = DateTime.Now;
        }
    }
}
