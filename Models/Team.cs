using System.ComponentModel.DataAnnotations;

public class Team
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    // Właściciel zespołu (relacja jeden-do-jednego)
    public int OwnerId { get; set; } // Klucz obcy do tabeli Users
    public User Owner { get; set; }  // Nawigacja do właściciela

    // Członkowie zespołu (relacja jeden-do-wielu)
    public List<User> Members { get; set; } = new List<User>(); // Nawigacja do członków zespołu
}