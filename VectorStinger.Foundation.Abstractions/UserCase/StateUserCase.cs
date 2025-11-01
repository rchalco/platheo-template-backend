using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Foundation.Abstractions.UserCase
{
    public enum StateUserCase
    {
        None = 0,
        Initialized = 1,
        ValidationError = 2,
        Completed = 3,
        Crushed = 4,
    }
}
