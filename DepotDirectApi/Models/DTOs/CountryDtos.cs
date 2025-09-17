namespace DepotDirectApi.Models.DTOs;

public class CountryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IsoCode { get; set; }
    public object? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastUpdatedBy { get; set; }
}

public class CountryCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? IsoCode { get; set; }
    public object? Metadata { get; set; }
}

public class CountryUpdateDto
{
    public string? Name { get; set; }
    public string? IsoCode { get; set; }
    public object? Metadata { get; set; }
}

public class CountryWithStatsDto : CountryDto
{
    public int CompaniesCount { get; set; }
    public int RegionsCount { get; set; }
    public int DepotsCount { get; set; }
}

public class PagedResult<T>
{
    public List<T> Data { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}