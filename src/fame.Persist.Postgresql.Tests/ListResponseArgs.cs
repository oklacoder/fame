using System.Collections.Generic;

namespace fame.Persist.Postgresql.Tests
{
    public class ListResponseArgs<T> : 
        BaseResponseArgs
    {
        public IEnumerable<T> Values { get; set; }

        public ListResponseArgs(
            IEnumerable<T> values)
        {
            Values = values;
        }
    }
}
