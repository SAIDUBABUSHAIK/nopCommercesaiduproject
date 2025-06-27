using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.FilterLevels;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components;

public partial class CompatibleWithFilterLevelValuesViewComponent : NopViewComponent
{
    protected readonly IFilterLevelValueModelFactory _filterLevelValueModelFactory;
    protected readonly IFilterLevelValueService _filterLevelValueService;
    protected readonly ISettingService _settingService;


    public CompatibleWithFilterLevelValuesViewComponent(IFilterLevelValueModelFactory filterLevelValueModelFactory,
        IFilterLevelValueService filterLevelValueService,
        ISettingService settingService)
    {
        _filterLevelValueModelFactory = filterLevelValueModelFactory;
        _filterLevelValueService = filterLevelValueService;
        _settingService = settingService;
    }

    public async Task<IViewComponentResult> InvokeAsync(int productId)
    {
        var filterLevelSettings = await _settingService.LoadSettingAsync<FilterLevelSettings>();
        if (filterLevelSettings.FilterLevelEnabled && filterLevelSettings.DisplayOnProductDetailsPage)
        {
            //get filter level values
            var filterLevelValues = (await _filterLevelValueService
                .GetFilterLevelValuesByProductIdAsync(productId: productId));            

            var model = (await _filterLevelValueModelFactory.PrepareFilterLevelValueOverviewModelsAsync(filterLevelValues));

            return View(model);
        }

        return Content(string.Empty);
    }
}
