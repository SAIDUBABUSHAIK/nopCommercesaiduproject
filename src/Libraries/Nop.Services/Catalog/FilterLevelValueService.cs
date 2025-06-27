using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.FilterLevels;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Configuration;

namespace Nop.Services.Catalog;

/// <summary>
/// Filter level value service
/// </summary>
public partial class FilterLevelValueService : IFilterLevelValueService
{
    #region Fields

    protected readonly IRepository<FilterLevelValue> _filterLevelValueRepository;
    protected readonly IRepository<FilterLevelValueProductMapping> _filterLevelValueProductMappingRepository;
    protected readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
    protected readonly IRepository<Product> _productRepository;
    protected readonly ISettingService _settingService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public FilterLevelValueService(
        IRepository<FilterLevelValue> filterLevelValueRepository,
        IRepository<FilterLevelValueProductMapping> filterLevelValueProductMappingRepository,
        IRepository<LocalizedProperty> localizedPropertyRepository,
        IRepository<Product> productRepository,
        ISettingService settingService,
        IWorkContext workContext)
    {
        _filterLevelValueRepository = filterLevelValueRepository;
        _filterLevelValueProductMappingRepository = filterLevelValueProductMappingRepository;
        _localizedPropertyRepository = localizedPropertyRepository;
        _productRepository = productRepository;
        _settingService = settingService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Check if filter levels are disabled
    /// </summary>
    public async Task<(bool filterLevel1, bool filterLevel2, bool filterLevel3)> IsFilterLevelDisabledAsync()
    {
        var filterLevelSettings = await _settingService.LoadSettingAsync<FilterLevelSettings>();
        var filterLevelEnumDisabled = filterLevelSettings.FilterLevelEnumDisabled;

        return (filterLevelEnumDisabled.Contains((int)FilterLevelEnum.FilterLevel1),
                filterLevelEnumDisabled.Contains((int)FilterLevelEnum.FilterLevel2),
                filterLevelEnumDisabled.Contains((int)FilterLevelEnum.FilterLevel3));
    }

    /// <summary>
    /// Gets filter level values
    /// </summary>
    /// <param name="filterLevel1Value">Filter level 1 value</param>
    /// <param name="filterLevel2Value">Filter level 2 value</param>
    /// <param name="filterLevel3Value">Filter level 3 value</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filter level value list model
    /// </returns>
    public async Task<IPagedList<FilterLevelValue>> GetAllFilterLevelValuesAsync(string filterLevel1Value = null, string filterLevel2Value = null, string filterLevel3Value = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var (filterLevel1Disabled, filterLevel2Disabled, filterLevel3Disabled) = await IsFilterLevelDisabledAsync();

        var filterLevelValues = await _filterLevelValueRepository.GetAllAsync(query =>
        {
            //filter by filter level 1 value
            if (!string.IsNullOrEmpty(filterLevel1Value) && filterLevel1Value != "0" && !filterLevel1Disabled)
                query = query.Where(pc => pc.FilterLevel1Value.Contains(filterLevel1Value));

            //filter by filter level 2 value
            if (!string.IsNullOrEmpty(filterLevel2Value) && filterLevel2Value != "0" && !filterLevel2Disabled)
                query = query.Where(pc => pc.FilterLevel2Value.Contains(filterLevel2Value));

            //filter by filter level 3 value
            if (!string.IsNullOrEmpty(filterLevel3Value) && filterLevel3Value != "0" && !filterLevel3Disabled)
                query = query.Where(pc => pc.FilterLevel3Value.Contains(filterLevel3Value));

            return query;
        });

        var orderedFilterLevelValues = filterLevelValues.OrderBy(pc => pc.FilterLevel1Value)
            .ThenBy(pc => pc.FilterLevel2Value)
            .ThenBy(pc => pc.FilterLevel3Value)
            .ToList();

        return new PagedList<FilterLevelValue>(orderedFilterLevelValues, pageIndex, pageSize);
    }

    /// <summary>
    /// Inserts Filter level value
    /// </summary>
    /// <param name="filterLevelValue">Filter level value</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertFilterLevelValueAsync(FilterLevelValue filterLevelValue)
    {
        await _filterLevelValueRepository.InsertAsync(filterLevelValue);
    }

    /// <summary>
    /// Gets a filter level value
    /// </summary>
    /// <param name="filterLevelValueId">Filter level value identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filter level value
    /// </returns>
    public virtual async Task<FilterLevelValue> GetFilterLevelValueByIdAsync(int filterLevelValueId)
    {
        return await _filterLevelValueRepository.GetByIdAsync(filterLevelValueId, cache => default);
    }

    /// <summary>
    /// Gets filter level values by product identifier
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filter level values
    /// </returns>
    public virtual async Task<IList<FilterLevelValue>> GetFilterLevelValuesByProductIdAsync(int productId,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = from flv_map in _filterLevelValueProductMappingRepository.Table
                    join flv in _filterLevelValueRepository.Table on flv_map.FilterLevelValueId equals flv.Id
                    where flv_map.ProductId == productId
                    orderby flv.Id
                    select flv;

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets filter level values by identifier
    /// </summary>
    /// <param name="filterLevelValueIds">Filter level value identifiers</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filter level values
    /// </returns>
    public virtual async Task<IList<FilterLevelValue>> GetFilterLevelValuesByIdsAsync(int[] filterLevelValueIds)
    {
        return await _filterLevelValueRepository.GetByIdsAsync(filterLevelValueIds, includeDeleted: false);
    }

    /// <summary>
    /// Updates the filter level value
    /// </summary>
    /// <param name="filterLevelValue">Filter level value</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task UpdateFilterLevelValueAsync(FilterLevelValue filterLevelValue)
    {
        ArgumentNullException.ThrowIfNull(filterLevelValue);

        await _filterLevelValueRepository.UpdateAsync(filterLevelValue);
    }

    /// <summary>
    /// Delete filter level value
    /// </summary>
    /// <param name="filterLevelValue">Filter level value</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteFilterLevelValueAsync(FilterLevelValue filterLevelValue)
    {
        await _filterLevelValueRepository.DeleteAsync(filterLevelValue);
    }

    /// <summary>
    /// Delete filter level values
    /// </summary>
    /// <param name="filterLevelValues">Filter level values</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteFilterLevelValuesAsync(IList<FilterLevelValue> filterLevelValues)
    {
        ArgumentNullException.ThrowIfNull(filterLevelValues);

        foreach (var filterLevelValue in filterLevelValues)
            await DeleteFilterLevelValueAsync(filterLevelValue);
    }

    #region Mapping

    /// <summary>
    /// Gets products collection by filter level value identifier
    /// </summary>
    /// <param name="filterLevelValueId">Filter level value identifier</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="orderBy">Order by</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the products collection
    /// </returns>
    public virtual async Task<IPagedList<Product>> GetProductsByFilterLevelValueIdAsync(int filterLevelValueId,
        int pageIndex = 0, 
        int pageSize = int.MaxValue,
        ProductSortingEnum orderBy = ProductSortingEnum.Position)
    {
        if (filterLevelValueId == 0)
            return new PagedList<Product>(new List<Product>(), pageIndex, pageSize);

        var query = from pc in _filterLevelValueProductMappingRepository.Table
                    join p in _productRepository.Table on pc.ProductId equals p.Id
                    where pc.FilterLevelValueId == filterLevelValueId && !p.Deleted
                    orderby pc.Id
                    select p;

        return await query.OrderBy(_localizedPropertyRepository, await _workContext.GetWorkingLanguageAsync(), orderBy).ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets filter level value product mapping collection
    /// </summary>
    /// <param name="filterLevelValueId">Filter level value identifier</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filter level value product mapping collection
    /// </returns>
    public virtual async Task<IPagedList<FilterLevelValueProductMapping>> GetFilterLevelValueProductsByFilterLevelValueIdAsync(int filterLevelValueId,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        if (filterLevelValueId == 0)
            return new PagedList<FilterLevelValueProductMapping>(new List<FilterLevelValueProductMapping>(), pageIndex, pageSize);

        var query = from pc in _filterLevelValueProductMappingRepository.Table
                    join p in _productRepository.Table on pc.ProductId equals p.Id
                    where pc.FilterLevelValueId == filterLevelValueId && !p.Deleted
                    orderby pc.Id
                    select pc;

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets a filter level value product mapping
    /// </summary>
    /// <param name="filterLevelValueId">Filter level value identifier</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the filter level value product mapping
    /// </returns>
    public virtual async Task<FilterLevelValueProductMapping> GetFilterLevelValueProductByIdAsync(int filterLevelValueProductId)
    {
        return await _filterLevelValueProductMappingRepository.GetByIdAsync(filterLevelValueProductId, cache => default);
    }

    /// <summary>
    /// Deletes a filter level value product mapping
    /// </summary>
    /// <param name="filterLevelValueProduct">Filter level value product mapping</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task DeleteFilterLevelValueProductAsync(FilterLevelValueProductMapping filterLevelValueProduct)
    {
        await _filterLevelValueProductMappingRepository.DeleteAsync(filterLevelValueProduct);
    }

    /// <summary>
    /// Returns a FilterLevelValueProductMapping that has the specified values
    /// </summary>
    /// <param name="source">Source</param>
    /// <param name="productId">Product identifier</param>
    /// <param name="filterLevelValueId">Filter level value identifier</param>
    /// <returns>A FilterLevelValueProductMapping that has the specified values; otherwise null</returns>
    public virtual FilterLevelValueProductMapping FindFilterLevelValueProduct(IList<FilterLevelValueProductMapping> source, int productId, int filterLevelValueId)
    {
        return source.FirstOrDefault(pc => pc.ProductId == productId && pc.FilterLevelValueId == filterLevelValueId);
    }

    /// <summary>
    /// Inserts a filter level value product mapping
    /// </summary>
    /// <param name="filterLevelValueProduct">Filter level value product mapping</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task InsertProductFilterLevelValueAsync(FilterLevelValueProductMapping filterLevelValueProduct)
    {
        await _filterLevelValueProductMappingRepository.InsertAsync(filterLevelValueProduct);
    }

    #endregion

    #endregion
}
