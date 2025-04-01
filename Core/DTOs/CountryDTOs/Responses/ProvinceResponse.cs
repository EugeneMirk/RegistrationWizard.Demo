using Domain.Entities;

namespace Core.DTOs.CountryDTOs.Responses;

public record ProvinceResponse
{
    public int Id
    { get; set; }

    public string Name
    { get; set; } = null!;

    public int CountryId
    { get; set; }


    public ProvinceResponse()
    { }

    public ProvinceResponse(Province province)
    {
        Id = province.Id;
        Name = province.Name;
        CountryId = province.CountryId;
    }
}
