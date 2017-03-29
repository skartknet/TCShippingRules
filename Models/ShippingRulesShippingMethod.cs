using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace Koben.TeaCommerce.Models
{
    public class ShippingRulesShippingMethod : PublishedContentModel
    {
        public long StoreId { get; set; }
        public long ShippingMethodId { get; set; }

        public ShippingRulesShippingMethod(IPublishedContent content)
            : base(content)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<ShippingRulesShippingMethod>(content.GetPropertyValue<string>("tCShippingLink"));
                StoreId = data.StoreId;
                ShippingMethodId = data.ShippingMethodId;
            }
            catch { }
        }
    }
}
