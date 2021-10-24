using System;

namespace fame
{
    public class BaseEvent :
        IMessage
    {
        public Guid RefId { get; set; }
        public DateTime DateTimeUtc { get; set; }
        public virtual BaseEventArgs Args { get; set; }
        public Guid SourceId { get; set; }
        public string SourceUserId { get; set; }

        public BaseEvent()
        {
            RefId = Guid.NewGuid();
            DateTimeUtc = DateTime.Now;
        }
    }
}
