using System;
using System.Collections.Generic;

namespace fame.Persist.Postgresql.Tests
{
    public class ListResponse :
        BaseResponse
    {

        public ListResponse()
             : base()
        {
            Messages = new List<string>();
        }
        public static ListResponse Success<T>(
            ListResponseArgs<T> args)
        {
            return new ListResponse()
            {
                Successful = true,
                IsValid = true,
                Args = args,
                Messages = Array.Empty<string>()
            };
        }

        public static ListResponse Invalid(
            IEnumerable<string> messages)
        {
            return new ListResponse 
            {
                IsValid = false,
                Successful = false,
                Messages = messages
            };
        }
        public static ListResponse Error(
            IEnumerable<string> messages)
        {
            return new ListResponse
            {
                Successful = false,
                Messages = messages
            };
        }
    }
}
