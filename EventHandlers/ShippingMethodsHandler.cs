using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Koben.TeaCommerce.EventHandlers
{
    public class ShippingMethodsHandler : ApplicationEventHandler
    {
        private ApplicationContext _appContext;
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            _appContext = applicationContext;
            ContentService.Publishing += ContentService_Publishing;
        }

        private void ContentService_Publishing(Umbraco.Core.Publishing.IPublishingStrategy sender, Umbraco.Core.Events.PublishEventArgs<Umbraco.Core.Models.IContent> e)
        {
            //Checks if the Shipping Rule saving overlaps any other on the same shipping method.
            var rules = e.PublishedEntities.Where(ent => ent.ContentType.Alias == "shippingCostByWeightRule");
            foreach (var rule in rules)
            {
                
                //we get siblings of type shippingCostByWeightRule. Maybe in the future we have more rules types?
                var siblings = _appContext.Services.ContentService.GetChildren(rule.ParentId).Where(c => c.ContentType.Alias == "shippingCostByWeightRule" && c.Id != rule.Id);

                if (siblings.Any(s => (int)rule.Properties["fromWeight"].Value <= (int)s.Properties["toWeight"].Value && (int)rule.Properties["toWeight"].Value >= (int)s.Properties["fromWeight"].Value))
                {
                    //we found an overlapping rule!                      
                    e.CancelOperation(new Umbraco.Core.Events.EventMessage("Conflicting Rules", "The range of this rule conflicts with an existing one. The rule can't be published.", Umbraco.Core.Events.EventMessageType.Warning));
                }
            }
        }

    }
}
