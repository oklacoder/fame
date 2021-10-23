using System;

namespace fame
{
    public class BaseQuery :
        IMessage
    {
        public Guid RefId { get; set; }
        public DateTime DateTimeUtc { get; set; }
        public DateTime? CompletedDateUtc { get; set; }
        public DateTime? ErrorDateUtc { get; set; }

        public virtual BaseQueryArgs Args { get; set; }
        public string UserId { get; set; }

        public DateTime? FinishTime => (ErrorDateUtc ?? CompletedDateUtc);
        public long? DurationMs => FinishTime.HasValue ? (FinishTime - DateTimeUtc)?.Milliseconds : null;
    }
}
