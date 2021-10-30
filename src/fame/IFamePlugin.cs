using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fame
{
    public interface IFamePlugin
    {
        public static void Configure(IConfiguration config) 
        {
            throw new NotImplementedException("Interface method can't be called direclty - override in implementation.");
        }
        public static void Enroll(IOperator @operator) 
        {
            throw new NotImplementedException("Interface method can't be called direclty - override in implementation.");
        }
    }
}
