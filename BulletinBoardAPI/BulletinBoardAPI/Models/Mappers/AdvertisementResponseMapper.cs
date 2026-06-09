using BulletinBoardAPI.Models.Responses;

namespace BulletinBoardAPI.Models.Mappers;

public static class AdvertisementResponseMapper
{
    public static AdvertisementResponse ToResponse(this Advertisement advertisement, Category? category) =>
        new AdvertisementResponse
        {
            Id = advertisement.Id,
            Title = advertisement.Title,
            Description = advertisement.Description,
            Category = category,
            Price = advertisement.Price,
            Currency = advertisement.Currency,
            LastUpdate = advertisement.LastUpdate,
            Location = advertisement.Location,
            Contact = advertisement.Contact
        };

    public static List<AdvertisementResponse> ToResponseList(this List<Advertisement> advertisements, List<Category> categories)
    {
        var categoryMap = categories.ToDictionary(c => c.Id);
        return advertisements
            .Select(ad => ad.ToResponse(categoryMap.GetValueOrDefault(ad.CategoryId)))
            .ToList();
    }
}
