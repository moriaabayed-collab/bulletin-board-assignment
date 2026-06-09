namespace BulletinBoardAPI.Models.Responses;

public class AdvertisementResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Category? Category { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public DateTime? LastUpdate { get; set; }
    public string? Location { get; set; }
    public string Contact { get; set; } = string.Empty;
}
