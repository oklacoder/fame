using System;
using System.Linq;
using System.Text;

namespace fame
{
    public interface IMessage
    {
        public Guid RefId { get; set; }
        public DateTime DateTimeUtc { get; set; }
    }
}
