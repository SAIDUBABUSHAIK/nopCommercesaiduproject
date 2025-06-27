using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.FilterLevels;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Factories;

public partial class FilterLevelValueModelFactory : IFilterLevelValueModelFactory
{
    #region Fields

    protected readonly ICatalogModelFactory _catalogModelFactory;
    protected readonly IFilterLevelValueService _filterLevelValueService;
    protected readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public FilterLevelValueModelFactory(ICatalogModelFactory catalogModelFactory,
        IFilterLevelValueService filterLevelValueService,
        ILocalizationService localizationService)
    {
        _catalogModelFactory = catalogModelFactory;
        _filterLevelValueService = filterLevelValueService;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare the filter level value overview models
    /// </summary>
    /// <param name="filterLevelValues">Filter level values</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filter level value overview model
    /// </returns>
    public virtual async Task<FilterLevelValueOverviewModel> PrepareFilterLevelValueOverviewModelsAsync(IList<FilterLevelValue> filterLevelValues)
    {
        ArgumentNullException.ThrowIfNull(filterLevelValues);

        var (filterLevel1Disabled, filterLevel2Disabled, filterLevel3Disabled) = await _filterLevelValueService.IsFilterLevelDisabledAsync();

        var model = new FilterLevelValueOverviewModel();
        model.TotalFilterLevelValues = filterLevelValues.Count;
        model.FilterLevel1ValueEnabled = !filterLevel1Disabled;
        model.FilterLevel2ValueEnabled = !filterLevel2Disabled;
        model.FilterLevel3ValueEnabled = !filterLevel3Disabled;

        foreach (var filterLevelValue in filterLevelValues)
        {
            var itemModel = new FilterLevelValueInfoModel
            {
                FilterLevel1Value = filterLevelValue.FilterLevel1Value,
                FilterLevel2Value = filterLevelValue.FilterLevel2Value,
                FilterLevel3Value = filterLevelValue.FilterLevel3Value,
            };
            model.FilterLevelValues.Add(itemModel);
        }

        return model;
    }

    /// <summary>
    /// Prepare available filter level values
    /// </summary>
    /// <param name="items">Plugin group items</param>
    /// <param name="filterLevelValueEnum">Filter level value enum</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task PrepareFilterLevelValuesAsync(IList<SelectListItem> items, FilterLevelEnum filterLevelValueEnum, string filterLevelValue = null)
    {
        ArgumentNullException.ThrowIfNull(items);

        var param1 = filterLevelValueEnum == FilterLevelEnum.FilterLevel1 ? filterLevelValue : null;
        var param2 = filterLevelValueEnum == FilterLevelEnum.FilterLevel2 ? filterLevelValue : null;
        var param3 = filterLevelValueEnum == FilterLevelEnum.FilterLevel3 ? filterLevelValue : null;

        //prepare available filter level values
        var availableFilterLevelValues = (await _filterLevelValueService.GetAllFilterLevelValuesAsync(param1, param2, param3))
            .Select(filterLevelValue =>
            {
                // filter by filter level value enum
                switch (filterLevelValueEnum)
                {
                    case FilterLevelEnum.FilterLevel1:
                        return filterLevelValue.FilterLevel1Value;
                    case FilterLevelEnum.FilterLevel2:
                        return filterLevelValue.FilterLevel2Value;
                    case FilterLevelEnum.FilterLevel3:
                        return filterLevelValue.FilterLevel3Value;
                    default:
                        return string.Empty;
                }
            })
            .Distinct()
            .OrderBy(levelValue => levelValue)
            .ToList();

        foreach (var flv in availableFilterLevelValues)
            items.Add(new SelectListItem { Value = flv, Text = flv });

        //insert special item for the default value
        var defaultItemText = await _localizationService.GetResourceAsync("Admin.Common.Select");
        items.Insert(0, new SelectListItem { Text = defaultItemText, Value = "0" });
    }

    /// <summary>
    /// Prepare filter level value search model
    /// </summary>
    /// <param name="searchModel">Filter level value search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filter level value search model
    /// </returns>
    public virtual async Task<FilterLevelValueSearchModel> PrepareFilterLevelValueSearchModelAsync(FilterLevelValueSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var (filterLevel1Disabled, filterLevel2Disabled, filterLevel3Disabled) = await _filterLevelValueService.IsFilterLevelDisabledAsync();

        //prepare filter (0 - all;)
        searchModel.HideSearchFilterValue1 = filterLevel1Disabled;
        await PrepareFilterLevelValuesAsync(searchModel.AvailableFilterLevel1Values, FilterLevelEnum.FilterLevel1);

        searchModel.HideSearchFilterValue2 = filterLevel2Disabled;
        searchModel.HideSearchFilterValue3 = filterLevel3Disabled;

        return searchModel;
    }

    /// <summary>
    /// Prepare search filter level value model
    /// </summary>
    /// <param name="model">Search filter level value model</param>
    /// <param name="command">Model to get the catalog products</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the search filter level value model
    /// </returns>
    public virtual async Task<SearchFilterLevelValueModel> PrepareSearchFilterLevelValueModelAsync(SearchFilterLevelValueModel model, CatalogProductsCommand command)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(command);

        var (filterLevel1Disabled, filterLevel2Disabled, filterLevel3Disabled) = await _filterLevelValueService.IsFilterLevelDisabledAsync();
        model.HideSearchFilterValue1 = filterLevel1Disabled;
        model.HideSearchFilterValue2 = filterLevel2Disabled;
        model.HideSearchFilterValue3 = filterLevel3Disabled;

        await PrepareFilterLevelValuesAsync(model.AvailableFilterLevel1Values, FilterLevelEnum.FilterLevel1);
        foreach (var item in model.AvailableFilterLevel1Values)
        {
            if (item.Value == model.fl1id)
                item.Selected = true;
        }

        if (!string.IsNullOrEmpty(model.fl1id))
        {
            await PrepareFilterLevelValuesAsync(model.AvailableFilterLevel2Values, FilterLevelEnum.FilterLevel2, model.fl2id);
            foreach (var item in model.AvailableFilterLevel2Values)
            {
                if (item.Value == model.fl2id)
                    item.Selected = true;
            }

            await PrepareFilterLevelValuesAsync(model.AvailableFilterLevel3Values, FilterLevelEnum.FilterLevel3, model.fl3id);
            foreach (var item in model.AvailableFilterLevel3Values)
            {
                if (item.Value == model.fl3id)
                    item.Selected = true;
            }
        }

        model.CatalogProductsModel = await _catalogModelFactory.PrepareSearchProductsByFilterLevelValuesModelAsync(model, command);

        return model;
    }

    #endregion
}
