using System.ComponentModel.DataAnnotations;

namespace apbd_cw8.Models;

public class Client
{
    public int IdClient { get; set; }

    [Required]
    [MaxLength(120)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(120)]
    public string LastName { get; set; }

    [Required]
    [MaxLength(120)]
    public string Email { get; set; }

    [Required]
    [MaxLength(120)]
    public string Telephone { get; set; }

    [Required]
    [MaxLength(120)]
    public string Pesel { get; set; }
}