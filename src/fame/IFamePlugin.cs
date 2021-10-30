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
        public void Configure(IConfiguration config, ILoggerFactory logger) 
        {
            throw new NotImplementedException("Interface method can't be called direclty - override in implementation.");
        }
        public void Enroll(IOperator @operator) 
        {
            throw new NotImplementedException("Interface method can't be called direclty - override in implementation.");
        }
    }
}
