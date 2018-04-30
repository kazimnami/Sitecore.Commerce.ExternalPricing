using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions.Extensions;

namespace Feature.Pricing.Engine
{
    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore()
                .Pipelines(config => config
                    .ConfigurePipeline<ICalculateCartLinesPipeline>(
                        d =>
                        {
                            d.Add<Feature.Pricing.Engine.CalculateCartLinesPriceBlock>().After<Sitecore.Commerce.Plugin.Catalog.CalculateCartLinesPriceBlock>();
                        }, order: 1100)
                );
        }
    }
}
