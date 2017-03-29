using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Koben.TeaCommerce.Models
{
    public class WeightRule
    {
        public RuleType Type { get; set; }
        public int FromWeight { get; set; }
        public int ToWeight { get; set; }
        public decimal Price { get; set; }
        


        public WeightRule(IPublishedContent node)
        {
            Type = (RuleType)node.GetPropertyValue<int>("ruleType");
            FromWeight = node.GetPropertyValue<int>("fromWeight");
            ToWeight = node.GetPropertyValue<int>("toWeight");
            Price = node.GetPropertyValue<decimal>("price");            
        }
    }


    public enum RuleType
    {
        //applies a fixed cost to the range represented by this rule
        ByRange,

        //applies a price by kilo to the range represented by this rule
        ByKilo
    }
}
