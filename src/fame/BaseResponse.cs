using System;
using System.Collections.Generic;

namespace fame
{
    public class BaseResponse :
        IMessage
    {
        public Guid RefId { get; set; }
        public DateTime DateTimeUtc { get; set; }
        public virtual BaseResponseArgs Args { get; set; }
        public Guid? SourceId { get; set; }
        public bool? Successful { get; set; }
        public bool? IsValid { get; set; }
        public IEnumerable<string> Messages { get; set; }


        public static BaseResponse Invalid(
            IEnumerable<string> messages = null,
            IMessage source = null)
        {
            return new BaseResponse
            {
                RefId = Guid.NewGuid(),
                DateTimeUtc = DateTime.UtcNow,
                SourceId = source?.RefId,
                IsValid = false,
                Successful = false,
                Messages = messages
            };
        }
        public static BaseResponse Error(
            IEnumerable<string> messages = null,
            IMessage source = null)
        {
            return new BaseResponse
            {
                RefId = Guid.NewGuid(),
                DateTimeUtc = DateTime.UtcNow,
                SourceId = source?.RefId,
                Successful = false,
                Messages = messages
            };
        }
        public static BaseResponse Success(
            IMessage source = null)
        {
            return new BaseResponse
            {
                RefId = Guid.NewGuid(),
                DateTimeUtc = DateTime.UtcNow,
                SourceId = source?.RefId,
                IsValid = true,
                Successful = true
            };
        }
    }
}
