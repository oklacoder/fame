using System;
using System.Collections.Generic;

namespace fame
{

    public class BaseResponse :
        BaseMessage
    {
        public override Guid RefId { get; set; }
        public override DateTime DateTimeUtc { get; set; }
        public virtual BaseResponseArgs Args { get; set; }
        public Guid? SourceId { get; set; }
        public bool? Successful { get; set; }
        public bool? IsValid { get; set; }
        public IEnumerable<string> Messages { get; set; }

        public string Type
        {
            get { return GetType().FullName; }
            set { }
        }
        public string ArgsType
        {
            get { return Args?.GetType().FullName; }
            set { }
        }

        public BaseResponse()
        {
            RefId = Guid.NewGuid();
            DateTimeUtc = DateTime.UtcNow;
        }

        public BaseResponse(
            Guid sourceId,
            BaseResponseArgs args)
        {
            RefId = Guid.NewGuid();
            DateTimeUtc = DateTime.UtcNow;
            SourceId = sourceId;
            Args = args;
        }
    }
}
