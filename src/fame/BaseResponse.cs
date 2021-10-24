using System;
using System.Collections.Generic;

namespace fame
{
    public static class ResponseExtensions
    {
        public static T Invalid<T>(
            this T resp,
            IEnumerable<string> messages = null,
            IMessage source = null)
            where T : BaseResponse
        {
            resp.IsValid = false;
            resp.Successful = false;
            resp.Messages = messages;
            resp.SourceId = source?.RefId;

            return resp;
        }

        public static T Error<T>(
            this T resp,
            IEnumerable<string> messages = null,
            IMessage source = null)
            where T : BaseResponse
        {
            resp.Successful = false;
            resp.Messages = messages;
            resp.SourceId = source?.RefId;

            return resp;
        }
        public static T Success<T, TArgs>(
            this T resp,
            TArgs args = null,
            IMessage source = null)
            where T : BaseResponse
            where TArgs : BaseResponseArgs
        {
            resp.IsValid = true;
            resp.Successful = true;
            resp.Args = args;
            resp.SourceId = source?.RefId;

            return resp;
        }
    }

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

        public BaseResponse()
        {
            RefId = Guid.NewGuid();
            DateTimeUtc = DateTime.Now;
        }

        //public static BaseResponse Invalid(
        //    IEnumerable<string> messages = null,
        //    IMessage source = null)
        //{
        //    return new BaseResponse
        //    {
        //        RefId = Guid.NewGuid(),
        //        DateTimeUtc = DateTime.UtcNow,
        //        SourceId = source?.RefId,
        //        IsValid = false,
        //        Successful = false,
        //        Messages = messages
        //    };
        //}
        //public static BaseResponse Error(
        //    IEnumerable<string> messages = null,
        //    IMessage source = null)
        //{
        //    return new BaseResponse
        //    {
        //        RefId = Guid.NewGuid(),
        //        DateTimeUtc = DateTime.UtcNow,
        //        SourceId = source?.RefId,
        //        Successful = false,
        //        Messages = messages
        //    };
        //}
        //public static BaseResponse Success(
        //    IMessage source = null)
        //{
        //    return new BaseResponse
        //    {
        //        RefId = Guid.NewGuid(),
        //        DateTimeUtc = DateTime.UtcNow,
        //        SourceId = source?.RefId,
        //        IsValid = true,
        //        Successful = true
        //    };
        //}
    }
}
