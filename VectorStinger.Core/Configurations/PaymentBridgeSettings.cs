using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Core.Configurations
{
    public class PaymentBridgeSettings
    {
        public string Provider { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
