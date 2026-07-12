using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Configuration;
using ETicaret.Business.DTOs;
using ETicaret.Business.Infrastructure.Search.Models;
using ETicaret.Business.Interfaces;

namespace ETicaret.Business.Infrastructure.Search;

public class ElasticsearchService : ISearchService
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;

    public ElasticsearchService(ElasticsearchClient client, IConfiguration config)
    {
        _client = client;
        _indexName = config["Elasticsearch:DefaultIndex"] ?? "products";
    }

    public async Task IndexProductAsync(ProductDto product)
    {
        var doc = new ProductDocument
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description ?? "",
            CategoryName = product.CategoryName,
            Brand = product.Brand,
            Price = product.Price,
            AverageRating = (float)product.AverageRating,
            StockQuantity = product.StockQuantity,
            IsFeatured = product.IsFeatured,
            CreatedAt = product.CreatedAt
        };

        await _client.IndexAsync(doc, idx => idx.Index(_indexName).Id(doc.Id.ToString()));
    }

    public async Task DeleteProductIndexAsync(Guid productId)
    {
        await _client.DeleteAsync<ProductDocument>(productId.ToString(), d => d.Index(_indexName));
    }

    public async Task<ProductSearchResultDto> SearchAsync(ProductSearchFilterDto filters)
    {
        var mustQueries = new List<Action<QueryDescriptor<ProductDocument>>>();

        if (!string.IsNullOrWhiteSpace(filters.Query))
        {
            mustQueries.Add(q => q.QueryString(qs => qs
                .Fields(new[] { "title^3", "description", "categoryName^2", "brand^2" })
                .Query($"*{filters.Query}*")
            ));
        }

        if (filters.CategoryId.HasValue)
        {
            mustQueries.Add(q => q.Term(t => t.Field("categoryId").Value(filters.CategoryId.Value)));
        }
        
        if (!string.IsNullOrWhiteSpace(filters.Brand))
        {
            mustQueries.Add(q => q.Term(t => t.Field(f => f.Brand).Value(filters.Brand)));
        }
        
        if (filters.InStock == true)
        {
            mustQueries.Add(q => q.Range(r => r.NumberRange(nr => nr.Field(f => f.StockQuantity).Gt(0))));
        }
        
        if (filters.IsFeatured == true)
        {
            mustQueries.Add(q => q.Term(t => t.Field(f => f.IsFeatured).Value(true)));
        }

        var searchResponse = await _client.SearchAsync<ProductDocument>(s => s
            .Indices(_indexName)
            .From((filters.Page - 1) * filters.PageSize)
            .Size(filters.PageSize)
            .Query(q => q.Bool(b => b.Must(mustQueries.ToArray())))
            .Sort(srt => 
            {
                if (filters.SortBy == "price_asc")
                    srt.Field(f => f.Price, sort => sort.Order(SortOrder.Asc));
                else if (filters.SortBy == "price_desc")
                    srt.Field(f => f.Price, sort => sort.Order(SortOrder.Desc));
                else if (filters.SortBy == "rating")
                    srt.Field(f => f.AverageRating, sort => sort.Order(SortOrder.Desc));
                else
                    srt.Field(f => f.CreatedAt, sort => sort.Order(SortOrder.Desc));
            })
        );

        var result = new ProductSearchResultDto
        {
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalCount = (int)searchResponse.Total,
            Items = searchResponse.Documents.Select(d => new ProductDto(
                Id: d.Id,
                CategoryId: 0,
                CategoryName: d.CategoryName,
                Title: d.Title,
                Slug: "",
                Description: d.Description,
                Price: d.Price,
                DiscountedPrice: null,
                StockQuantity: d.StockQuantity,
                Brand: d.Brand,
                IsActive: true,
                IsFeatured: d.IsFeatured,
                AverageRating: d.AverageRating,
                ReviewCount: 0,
                SalesCount: 0,
                ImageUrls: new List<string>(),
                CreatedAt: d.CreatedAt
            )).ToList()
        };

        return result;
    }
}
