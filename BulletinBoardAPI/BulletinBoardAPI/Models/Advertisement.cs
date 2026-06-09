using System.ComponentModel.DataAnnotations;

namespace BulletinBoardAPI.Models;

public class Advertisement
{
    public int Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 2)]
    [RegularExpression(@"^[^<>'"";`]*$", ErrorMessage = "Title contains invalid characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    [RegularExpression(@"^[^<>'"";`]*$", ErrorMessage = "Description contains invalid characters.")]
    public string? Description { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Price { get; set; }

    [StringLength(30)]
    [RegularExpression(@"^[^<>'""\;\*\=\(\)]*$", ErrorMessage = "Currency contains invalid characters.")]
    public string? Currency { get; set; }

    public DateTime? LastUpdate { get; set; }

    [StringLength(200)]
    [RegularExpression(@"^[^<>'"";\-\/\*\=\(\)]*$", ErrorMessage = "Location contains invalid characters.")]
    public string? Location { get; set; }

    [Required]
    [StringLength(50)]
    public string Contact { get; set; } = string.Empty;
}
