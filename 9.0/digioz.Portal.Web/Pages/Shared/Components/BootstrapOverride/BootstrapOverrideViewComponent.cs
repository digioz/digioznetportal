using System.Threading.Tasks;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace digioz.Portal.Web.Pages.Shared.Components.BootstrapOverride
{
    public class BootstrapOverrideViewComponent : ViewComponent
    {
        private readonly IConfigService _configService;

        public BootstrapOverrideViewComponent(IConfigService configService)
        {
            _configService = configService;
        }

        public Task<IViewComponentResult> InvokeAsync()
        {
            // Get the Bootstrap override CSS from config
            var bootstrapOverrideConfig = _configService.GetByKey("BootstrapOverride");
            
            string customCss = null;
            if (bootstrapOverrideConfig != null && !string.IsNullOrWhiteSpace(bootstrapOverrideConfig.ConfigValue))
            {
                customCss = bootstrapOverrideConfig.ConfigValue;
            }

            return Task.FromResult<IViewComponentResult>(View((object)customCss));
        }
    }
}
