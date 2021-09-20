using System;
using MediatR;

namespace EventBus.Abstract
{
    public abstract class Message : IRequest<bool>
    {
        public string MessageType { get; protected set; }
        public Message()
        {
            MessageType = GetType().Name;
        }
    }
}
