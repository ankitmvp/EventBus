using System;
using System.IO;
using System.Threading.Tasks;
using EventBus.Abstract;
using EventBus.Contract;

namespace FileCreationService
{
    public class FileCreateEventHandler : IEventHandler<FileCreateEvent>
    {
        private const string _path = "/Users/ankitmvp/TestFiles";

        public Task Handle(FileCreateEvent @event)
        {
            var content = @event._data;
            Directory.CreateDirectory(_path);
            File.WriteAllText(Path.Combine(_path, $"{@event.Timestamp.ToLongDateString()}.json"), content);

            return Task.CompletedTask;
        }
    }
    public class FileCreateEvent : Event
    {
        public string _data { get; private set; }
        public FileCreateEvent(string _data)
        {
            this._data = _data;
        }
    }
}
