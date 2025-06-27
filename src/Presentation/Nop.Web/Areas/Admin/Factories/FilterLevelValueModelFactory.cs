using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.FilterLevels;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories;

/// <summary>
/// Represents the filter level value model factory implementation
/// </summary>
public partial class FilterLevelValueModelFactory : IFilterLevelValueModelFactory
{
    #region Fields

    protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
    protected readonly IFilterLevelValueService _filterLevelValueService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IProductService _productService;
    protected readonly ISettingService _settingService;
    protected readonly IUrlRecordService _urlRecordService;

    #endregion

    #region Ctor

    public FilterLevelValueModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
        IFilterLevelValueService filterLevelValueService, 
        ILocalizationService localizationService, 
        IProductService productService,
        ISettingService settingService,
        IUrlRecordService urlRecordService)
    {
        _baseAdminModelFactory = baseAdminModelFactory;
        _filterLevelValueService = filterLevelValueService;
        _localizationService = localizationService;
        _productService = productService;
        _settingService = settingService;
        _urlRecordService = urlRecordService;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Prepare filter level value product search model
    /// </summary>
    /// <param name="searchModel">Filter level value product search model</param>
    /// <param name="filterLevelValue">Filter level value</param>
    /// <returns>Filter level value product search model</returns>
    protected virtual FilterLevelValueProductSearchModel PrepareFilterLevelValueProductSearchModel(FilterLevelValueProductSearchModel searchModel, FilterLevelValue filterLevelValue)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        ArgumentNullException.ThrowIfNull(filterLevelValue);

        searchModel.FilterLevelValueId = filterLevelValue.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }    

    #endregion

    #region Methods

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
        await _baseAdminModelFactory.PrepareFilterLevelValuesAsync(searchModel.AvailableFilterLevel1Values, FilterLevelEnum.FilterLevel1);

        searchModel.HideSearchFilterValue2 = filterLevel2Disabled;
        await _baseAdminModelFactory.PrepareFilterLevelValuesAsync(searchModel.AvailableFilterLevel2Values, FilterLevelEnum.FilterLevel2);

        searchModel.HideSearchFilterValue3 = filterLevel3Disabled;
        await _baseAdminModelFactory.PrepareFilterLevelValuesAsync(searchModel.AvailableFilterLevel3Values, FilterLevelEnum.FilterLevel3);

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare paged filter level value list model
    /// </summary>
    /// <param name="searchModel">Filter level value search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filter level value list model
    /// </returns>
    public virtual async Task<FilterLevelValueListModel> PrepareFilterLevelValueListModelAsync(FilterLevelValueSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        //get filter level values
        var filterLevelValues = await _filterLevelValueService.GetAllFilterLevelValuesAsync(searchModel.SearchFilterValue1Id, searchModel.SearchFilterValue2Id, searchModel.SearchFilterValue3Id,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new FilterLevelValueListModel().PrepareToGridAsync(searchModel, filterLevelValues, () =>
        {
            return filterLevelValues.Select(filterLevelValue =>
            {
                //fill in model values from the entity
                var filterLevelValueModel = filterLevelValue.ToModel<FilterLevelValueModel>();

                return filterLevelValueModel;
            }).ToAsyncEnumerable();
        });

        return model;
    }

    /// <summary>
    /// Prepare filter level value model
    /// </summary>
    /// <param name="model">Filter level value model</param>
    /// <param name="filterLevelValue">Filter level value</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filter level value model
    /// </returns>
    public virtual async Task<FilterLevelValueModel> PrepareFilterLevelValueModelAsync(FilterLevelValueModel model, FilterLevelValue filterLevelValue)
    {
        if (filterLevelValue != null)
        {
            //fill in model values from the entity
            if (model == null)
            {
                model = filterLevelValue.ToModel<FilterLevelValueModel>();
            }

            //prepare nested search model
            PrepareFilterLevelValueProductSearchModel(model.FilterLevelValueProductSearchModel, filterLevelValue);
        }

        var (filterLevel1Disabled, filterLevel2Disabled, filterLevel3Disabled) = await _filterLevelValueService.IsFilterLevelDisabledAsync();

        model.FilterLevel1ValueEnabled = !filterLevel1Disabled;
        model.FilterLevel2ValueEnabled = !filterLevel2Disabled;
        model.FilterLevel3ValueEnabled = !filterLevel3Disabled;

        return model;
    }

    /// <summary>
    /// Prepare paged filter level value product list model
    /// </summary>
    /// <param name="searchModel">Filter level value product search model</param>
    /// <param name="filterLevelValue">Filter level value</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filter level value product list model
    /// </returns>
    public virtual async Task<FilterLevelValueProductListModel> PrepareFilterLevelValueProductListModelAsync(FilterLevelValueProductSearchModel searchModel, FilterLevelValue filterLevelValue)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        ArgumentNullException.ThrowIfNull(filterLevelValue);

        //get 
        var filterLevelValueProducts = await _filterLevelValueService.GetFilterLevelValueProductsByFilterLevelValueIdAsync(filterLevelValue.Id,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new FilterLevelValueProductListModel().PrepareToGridAsync(searchModel, filterLevelValueProducts, () =>
        {
            return filterLevelValueProducts.SelectAwait(async filterLevelValueProduct =>
            {
                //fill in model values from the entity
                var filterLevelValueProductModel = filterLevelValueProduct.ToModel<FilterLevelValueProductModel>();

                //fill in additional values (not existing in the entity)
                filterLevelValueProductModel.ProductName = (await _productService.GetProductByIdAsync(filterLevelValueProduct.ProductId))?.Name;

                return filterLevelValueProductModel;
            });
        });

        return model;
    }

    /// <summary>
    /// Prepare product search model to add to the filter level value
    /// </summary>
    /// <param name="searchModel">Product search model to add to the filter level value</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the product search model to add to the filter level value
    /// </returns>
    public virtual async Task<AddProductToFilterLevelValueSearchModel> PrepareAddProductToFilterLevelValueSearchModelAsync(AddProductToFilterLevelValueSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //prepare available categories
        await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

        //prepare available manufacturers
        await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

        //prepare available stores
        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        //prepare available vendors
        await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

        //prepare available product types
        await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

        //prepare page parameters
        searchModel.SetPopupGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare paged product list model to add to the filter level value
    /// </summary>
    /// <param name="searchModel">Product search model to add to the filter level value</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the product list model to add to the filter level value
    /// </returns>
    public virtual async Task<AddProductToFilterLevelValueListModel> PrepareAddProductToFilterLevelValueListModelAsync(AddProductToFilterLevelValueSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get products
        var products = await _productService.SearchProductsAsync(showHidden: true,
            categoryIds: new List<int> { searchModel.SearchCategoryId },
            manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
            storeId: searchModel.SearchStoreId,
            vendorId: searchModel.SearchVendorId,
            productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare grid model
        var model = await new AddProductToFilterLevelValueListModel().PrepareToGridAsync(searchModel, products, () =>
        {
            return products.SelectAwait(async product =>
            {
                var productModel = product.ToModel<ProductModel>();

                productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                return productModel;
            });
        });

        return model;
    }    

    #endregion
}
