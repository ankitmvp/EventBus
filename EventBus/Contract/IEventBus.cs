﻿using System;
using System.Threading.Tasks;
using EventBus.Abstract;

namespace EventBus.Contract
{
    public interface IEventBus
    {
        Task SendCommand<T>(T command) where T : Command;
        void Publish<T>(T @event) where T : Event;
        void Subscribe<T, TH>() where T : Event where TH : IEventHandler<T>;
    }
}
