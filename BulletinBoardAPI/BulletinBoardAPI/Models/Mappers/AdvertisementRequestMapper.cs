using BulletinBoardAPI.Models.Requests;

namespace BulletinBoardAPI.Models.Mappers;

public static class AdvertisementRequestMapper
{
    public static Advertisement ToAdvertisement(this AdvertisementRequest request) =>
        new Advertisement
        {
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Price = request.Price,
            Currency = request.Currency,
            Location = request.Location,
            Contact = request.Contact,
            LastUpdate = DateTime.UtcNow
        };

    public static Advertisement ToAdvertisement(this AdvertisementRequest request, int id) =>
        new Advertisement
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Price = request.Price,
            Currency = request.Currency,
            Location = request.Location,
            Contact = request.Contact,
            LastUpdate = DateTime.UtcNow
        };
}
