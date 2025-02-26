using LicencjatUG.Server.Models;
using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(100)]
    public string Surname { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; }  // Login

    [Required]
    public byte[] PasswordHash { get; set; }  // Hasło w formie hashowanej

    [Required]
    public byte[] PasswordSalt { get; set; }  // Salt do hasła

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "NonTeamMember"; // Domyślnie każdy użytkownik nie należy do zespołu

    [Required]
    [MaxLength(100)]
    public string Position { get; set; }  // Stanowisko użytkownika

    // Klucz obcy do tabeli Teams (nullable)
    public int? TeamId { get; set; } // Nullable TeamId

    // Nawigacja do zespołu, do którego należy użytkownik
    public Team Team { get; set; }

    // Nawigacja do zespołów, których użytkownik jest właścicielem
    public List<Team> OwnedTeams { get; set; } = new List<Team>();
}