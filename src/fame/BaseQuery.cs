﻿using System;

namespace fame
{
    public class BaseQuery :
        BaseMessage
    {
        public override Guid RefId { get; set; }
        public override DateTime DateTimeUtc { get; set; }
        public DateTime? CompletedDateUtc { get; set; }
        public DateTime? ErrorDateUtc { get; set; }

        public virtual BaseQueryArgs Args { get; set; }
        public string UserId { get; set; }

        public DateTime? FinishTime => (ErrorDateUtc ?? CompletedDateUtc);
        public long? DurationMs => FinishTime.HasValue ? (FinishTime - DateTimeUtc)?.Milliseconds : null;

        public BaseQuery()
        {
            RefId = Guid.NewGuid();
            DateTimeUtc = DateTime.Now;
        }
    }
}
