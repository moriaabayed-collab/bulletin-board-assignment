using System.ComponentModel.DataAnnotations;

namespace BulletinBoardAPI.Models.Requests;

public class CategoryRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [RegularExpression(@"^[^<>'"";\-\/\*\=\(\)]*$", ErrorMessage = "Name contains invalid characters.")]
    public string Name { get; set; } = string.Empty;
}