using System.ComponentModel.DataAnnotations;

namespace BulletinBoardAPI.Models;

public class Category
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [RegularExpression(@"^[^<>'"";\-\/\*\=\(\)]*$", ErrorMessage = "Name contains invalid characters.")]

    public string Name { get; set; } = string.Empty;
}
