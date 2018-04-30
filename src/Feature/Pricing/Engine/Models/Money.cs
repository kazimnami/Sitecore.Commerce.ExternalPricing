using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feature.Pricing.Engine
{
    public class Money
    {
        public decimal ExTax { get; set; }
        public decimal Tax { get; set; }
        public decimal IncTax { get; set; }
        public Currency Currency { get; set; }
    }
}
