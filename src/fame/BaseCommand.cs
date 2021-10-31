using System;
using System.Collections.Generic;

namespace fame
{
    public class BaseCommand :
        IMessage
    {
        public Guid RefId { get; set; }
        public DateTime DateTimeUtc { get; set; }
        public DateTime? ValidationFailedDateUtc { get; set; }
        public DateTime? CompletedDateUtc { get; set; }
        public DateTime? ErrorDateUtc { get; set; }
        public Exception? ErrorDetails { get; set; }
        
        public string UserId { get; set; }
        
        public virtual BaseCommandArgs Args { get; set; }
        public virtual BaseResponse Response { get; set; }


        public DateTime? FinishedDateUtc => (ErrorDateUtc ?? ValidationFailedDateUtc ?? CompletedDateUtc);
        public long? DurationMs => FinishedDateUtc.HasValue ? (FinishedDateUtc - DateTimeUtc)?.Milliseconds : null;

        public BaseCommand()
        {
            RefId = Guid.NewGuid();
            DateTimeUtc = DateTime.Now;
        }
        public BaseCommand(
            string userId,
            BaseCommandArgs args)
        {
            RefId = Guid.NewGuid();
            DateTimeUtc = DateTime.UtcNow;
            UserId = userId;
            Args = args;
        }

        public BaseCommand(
            string userId,
            BaseCommandArgs args,
            BaseResponse response)
        {
            RefId = Guid.NewGuid();
            DateTimeUtc = DateTime.UtcNow;
            UserId = userId;
            Args = args;
            Response = response;
        }

        public virtual bool Validate(out IEnumerable<string> messages)
        {
            messages = Array.Empty<string>();
            //valid by default, but implentations can have their own logic
            //expect false if invalid, with optional out messages
            return true;
        }
    }
}
