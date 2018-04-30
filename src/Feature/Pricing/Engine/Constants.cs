using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feature.Pricing.Engine
{
    public static class Constants
    {
        public const string BaseName = "Feature.Pricing.";

        public static class Pipelines
        {
            public static class Blocks
            {
                public const string CalculateCartLinesPrice = BaseName + "Block.CalculateCartLinesPrice";
            }
        }
    }
}
