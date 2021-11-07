using System;
using System.Linq;
using System.Text;

namespace fame
{
    public interface IMessage
    {
        public long SequenceId { get; set; }
        public Guid RefId { get; set; }
        public DateTime DateTimeUtc { get; set; }
    }

    public class BaseMessage :
        IMessage
    {
        public virtual long SequenceId { get; set; }
        public virtual Guid RefId { get; set; }
        public virtual DateTime DateTimeUtc { get; set; }
    }
}
