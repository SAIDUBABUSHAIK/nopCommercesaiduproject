using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.FilterLevels;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Components;

public partial class FilterLevelValueSearchViewComponent : NopViewComponent
{
    protected readonly IFilterLevelValueModelFactory _filterLevelValueModelFactory;
    protected readonly IFilterLevelValueService _filterLevelValueService;
    protected readonly ISettingService _settingService;


    public FilterLevelValueSearchViewComponent(
        IFilterLevelValueModelFactory filterLevelValueModelFactory,
        IFilterLevelValueService filterLevelValueService,
        ISettingService settingService)
    {
        _filterLevelValueModelFactory = filterLevelValueModelFactory;
        _filterLevelValueService = filterLevelValueService;
        _settingService = settingService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var filterLevelSettings = await _settingService.LoadSettingAsync<FilterLevelSettings>();
        if (filterLevelSettings.FilterLevelEnabled && filterLevelSettings.DisplayOnHomePage)
        {
            var model = await _filterLevelValueModelFactory.PrepareFilterLevelValueSearchModelAsync(new FilterLevelValueSearchModel());

            return View(model);
        }

        return Content(string.Empty);
    }
}
