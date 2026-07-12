using ETicaret.Business.DTOs;

namespace ETicaret.Business.Interfaces;

public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(SearchFilterDto filters);
    Task IndexListingAsync(ListingDto listing);
    Task DeleteListingIndexAsync(Guid listingId);
}

