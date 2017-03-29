using Lux.Core.TeaCommerce;
using Lux.Core.TeaCommerce.Shipping.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TeaCommerce.Api.Dependency;
using TeaCommerce.Api.Models;
using TeaCommerce.Api.Notifications;
using TeaCommerce.Api.PriceCalculators;
using TeaCommerce.Api.Services;
using TeaCommerce.Umbraco.Web;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedContentModels;

namespace Koben.TeaCommerce
{
    [SuppressDependency("TeaCommerce.Api.PriceCalculators.IShippingCalculator", "TeaCommerce.Api")]
    public class CustomShippingCalculator : ShippingCalculator
    {

        private readonly UmbracoHelper _umbHelper;

        public CustomShippingCalculator(IVatGroupService vatGroupService)
          : base(vatGroupService)
        {
            _umbHelper = new UmbracoHelper(UmbracoContext.Current);
        }

        public override Price CalculatePrice(ShippingMethod shippingMethod, Currency currency, Order order)
        {
            

            //it will contain the applicable rule for this order.
            WeightRule weightRule = null;

            //We search for the rules container, 'shippingRules' DocType...
            var shippingRules_ShippingRulesContainer = _umbHelper.TypedContentAtRoot().FirstOrDefault(c => c.DocumentTypeAlias == "shippingRules");
            if (shippingRules_ShippingRulesContainer == null)
            {
                LogHelper.Warn(this.GetType(), "A container of type 'shippingRules' couldn't be found. Default prices will be applied.");
            }
            else
            {
                //...and from all its children we search for the shipping method that is linked to our TeaCommerce method.
                // so we get all shipping methods
                var shippingRules_ShippingMethods = shippingRules_ShippingRulesContainer.Children(method=>method.HasValue("tCShippingLink"));
                if (shippingRules_ShippingMethods == null)
                {
                    LogHelper.Warn(this.GetType(), "A linked shipping method rules node couldn't be found. Default prices will be applied.");
                }
                else
                {
                    //and find the one that fits the order
                    var shippingRules_ShippingMethod = shippingRules_ShippingMethods.Select(c => new ShippingRulesShippingMethod(c))
                                    .FirstOrDefault(m => m.ShippingMethodId == order.ShipmentInformation.ShippingMethodId);
                    
                    if (shippingRules_ShippingMethod == null)
                    {
                        LogHelper.Warn(this.GetType(), "A linked shipping method rules node couldn't be found. Default prices will be applied.");
                    }
                    else
                    {
                        weightRule = FindRuleForOrder(shippingRules_ShippingMethod, order);
                    }
                }
            }

            if (weightRule != null)
            {
                return CalculateShippingCostFromRule(order, weightRule);
            }
            else
            {
                ServicePrice servicePrice = shippingMethod.OriginalPrices.FirstOrDefault(p => p.CurrencyId == currency.Id && p.CountryId == order.ShipmentInformation.CountryId);

                return new Price(
                    servicePrice != null ? servicePrice.Value : 0.0m,
                    order.ShipmentInformation.VatRate,
                    TC.GetCurrency(order.StoreId, order.CurrencyId));
            }


        }

        private WeightRule FindRuleForOrder(ShippingRulesShippingMethod shippingRules_ShippingMethod, Order order)
        {
            WeightRule rule = null;
            //then we take all the rules applied to this shipping method
            var shippingRules_ShippingMethodRules = shippingRules_ShippingMethod.Children("shippingCostByWeightRule").Select(r => new WeightRule(r));
            if (shippingRules_ShippingMethodRules == null || !shippingRules_ShippingMethodRules.Any())
            {
                LogHelper.Warn(this.GetType(), "A linked shipping method was found but not rules exist. Default prices will be applied.");
            }
            else
            {
                var totalOrderWeight = GetOrderWeight(order);

                try
                {
                    //Everything is in place, we'll serach for the rule that apply to this order.
                    rule = shippingRules_ShippingMethodRules.Single(wr => wr.FromWeight <= totalOrderWeight && wr.ToWeight >= totalOrderWeight);
                }
                catch (InvalidOperationException ex)
                {
                    LogHelper.WarnWithException(this.GetType(), "More than one rule apply to this weight. Default prices will be apply.", ex);
                }

            }

            return rule;
        }



        //private Price CalculateShippingCostFromDefault(Order order)
        //{
        //    decimal calculatedPrice;

        //    calculatedPrice = order.ShipmentInformation.TotalPrice;


        //    Price price = new Price(
        //             calculatedPrice,
        //             order.ShipmentInformation.VatRate,
        //             TC.GetCurrency(order.StoreId, order.CurrencyId));
        //}

        private Price CalculateShippingCostFromRule(Order order, WeightRule rule)
        {
            if (rule == null) throw new NullReferenceException("rule");
            if (order == null) throw new NullReferenceException("order");

            decimal calculatedPrice;


            // order shipping cost set to rules cost unless the 'is per kg' override has been set
            if (rule.Type == RuleType.ByRange)
            {
                calculatedPrice = rule.Price;
            }
            else
            {
                calculatedPrice = rule.Price * Math.Ceiling((decimal)(GetOrderWeight(order) / 1000));
            }


            Price price = new Price(
                    calculatedPrice,
                    order.ShipmentInformation.VatRate,
                    TC.GetCurrency(order.StoreId, order.CurrencyId));

            return price;
        }

        /// <summary>
        /// Returns total order weight in grams.
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private int GetOrderWeight(Order order)
        {
            // Total weight of the order
            int totalWeightInGrams = 0;

            // Navigate through each order line, one order line may contain multiple products
            foreach (OrderLine ol in order.OrderLines)
            {                
                totalWeightInGrams += ((LuxProduct)_umbHelper.TypedContent(int.Parse(ol.ProductIdentifier))).Weight * (int)ol.Quantity;                
            }

            return totalWeightInGrams;
        }
    }
}