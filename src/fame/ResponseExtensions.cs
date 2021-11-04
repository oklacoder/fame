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
}
