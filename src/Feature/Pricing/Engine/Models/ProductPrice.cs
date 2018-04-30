using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feature.Pricing.Engine
{
    public class ProductPrice
    {
        public DateTime Expires { get; set; }
        public Money Price { get; set; }
        public string ResellerId { get; set; }
        public string Token { get; set; }
    }
}
