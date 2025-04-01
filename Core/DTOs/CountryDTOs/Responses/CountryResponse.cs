using Domain.Entities;

namespace Core.DTOs.CountryDTOs.Responses;

public record CountryResponse
{
    public int Id
    { get; init; }

    public string Name
    { get; init; }

    public CountryResponse(int id, string name)
    {
        Id = id;
        Name = name ?? string.Empty;
    }

    public CountryResponse(Country country)
    {
        Id = country.Id;
        Name = country.Name ?? string.Empty;
    }
}
