using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Feature.Pricing.Engine
{
    [PipelineDisplayName(Constants.Pipelines.Blocks.CalculateCartLinesPrice)]
    public class CalculateCartLinesPriceBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;

        public CalculateCartLinesPriceBlock(CommerceCommander commerceCommander)
          : base(null)
        {
            _commerceCommander = commerceCommander;
        }

        public override async Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: Cart cannot be null.");
            Condition.Requires(arg.Lines).IsNotNull($"{Name}: The cart's lines cannot be null");

            if (!arg.Lines.Any())
                return arg;

            // Valid cart items
            var validCartLines = arg.Lines
                .Where(line =>
                    line != null
                    && line.HasComponent<CartProductComponent>()
                    && !string.IsNullOrEmpty(line.ItemId)
                    && line.ItemId.Split('|').Length >= 2);

            // Iterate valid cart items
            foreach (var line in validCartLines)
            {
                // Get sellable items (products)
                SellableItem sellableItem = await _commerceCommander.Command<GetSellableItemCommand>().Process(context.CommerceContext, line.ItemId, false);
                if (sellableItem == null)
                {
                    context.Logger.LogError($"{Name}-SellableItemNotFound for Cart Line: ItemId={line.ItemId}|CartId={arg.Id}|LineId={line.Id}");
                    return arg;
                }

                // Set list & sell pricing
                line.UnitListPrice = sellableItem.ListPrice;

                if (sellableItem.Manufacturer.Equals("CMYKhub", StringComparison.OrdinalIgnoreCase))
                {
                    var request = new
                    {
                        ProductId = sellableItem.FriendlyId,
                        Quantity = (int)line.Quantity,
                        Kinds = 1
                    };

                    var cmykHubPrice = await CreatePriceAsync(request);
                    if (cmykHubPrice != null)
                    {
                        var optionMoneyPolicy = new PurchaseOptionMoneyPolicy
                        {
                            SellPrice = new Sitecore.Commerce.Core.Money("USD", cmykHubPrice.Price.IncTax) // new Sitecore.Commerce.Core.Money(cmykHubPrice.Price.Currency.Code, cmykHubPrice.Price.IncTax);
                        };

                        line.Policies.Remove(line.Policies.OfType<PurchaseOptionMoneyPolicy>().FirstOrDefault());
                        line.SetPolicy(optionMoneyPolicy);

                        // Add to pricing messaging
                        var lineMessages = line.GetComponent<MessagesComponent>();
                        lineMessages.AddMessage(context.GetPolicy<KnownMessageCodePolicy>().Pricing, $"CartItem.SellPrice<=CMYKhub.Pricing: Price={optionMoneyPolicy.SellPrice.AsCurrency()}");
                    }
                };
            }
            return arg;
        }

        public async Task<ProductPrice> CreatePriceAsync(object request)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.cmykhub+json"));
            client.DefaultRequestHeaders.Add("ResellerId", "4926");
            client.DefaultRequestHeaders.Add("APIKey", "xBZBrw6mDuS1bAA8G9E4quTmOHSlAPbRuyewj+0ujnY=");

            var response = await client.PostAsJsonAsync("https://hublink.api.cmykhub.com/man/Pricing/Standard", request);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<ProductPrice>();
            else
                return default(ProductPrice);
        }
    }


}