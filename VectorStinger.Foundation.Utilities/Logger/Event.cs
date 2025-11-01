using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Foundation.Utilities.Logger
{
    public class Event
    {
        public enum Category
        {
            Information = 0,
            Warning = 1,
            Error = 2
        }

        public Category category { get; set; }
        public string description { get; set; }
    }
}
