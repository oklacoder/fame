using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fame
{
    public interface IFamePlugin
    {
        public bool? IsConfigured { get; }
        bool? IsProcessing { get; }
        int? QueuedMessages { get; }

        public void Configure(IConfiguration config, ILoggerFactory logger);
        public void Enroll(IOperator @operator);
    }
}
