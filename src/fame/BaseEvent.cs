using System;

namespace fame
{
    public class BaseEvent :
        BaseMessage
    {
        public override Guid RefId { get; set; }
        public override DateTime DateTimeUtc { get; set; }
        public virtual BaseEventArgs Args { get; set; }
        public Guid SourceId { get; set; }
        public string SourceUserId { get; set; }
        public string SourceService { get; set; }

        public string AggregateId => Args?.AggregateId;

        public BaseEvent()
        {
            RefId = Guid.NewGuid();
            DateTimeUtc = DateTime.UtcNow;
        }

        public BaseEvent(
            Guid sourceId,
            string sourceUserId,
            string sourceService,
            BaseEventArgs args)
        {

            RefId = Guid.NewGuid();
            DateTimeUtc = DateTime.UtcNow;
            SourceId = sourceId;
            SourceUserId = sourceUserId;
            SourceService = sourceService;
            Args = args;
        }
    }
}
