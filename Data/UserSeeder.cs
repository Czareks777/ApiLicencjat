using LicencjatUG.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace LicencjatUG.Server.Data.Seeders
{
    public static class UserSeeder
    {
        public static async Task SeedUsersAsync(DataContext context)
        {
           if (await context.Users.AnyAsync()) return;
            Console.WriteLine("🔄 Wymuszone seedowanie użytkowników...");
           
            var users = new List<User>
            {
                // Użytkownicy przypisani do zespołu (wartość 1 zostanie zastąpiona identyfikatorem domyślnego zespołu)
                CreateUser("Anna", "Kowalska", "anna.kowalska", "Frontend Developer", 1),
                CreateUser("Jan", "Nowak", "jan.nowak", "Backend Developer", 1),
                CreateUser("Katarzyna", "Lis", "katarzyna.lis", "UX Designer", 1),
                CreateUser("Marek", "Wiśniewski", "marek.wisniewski", "Project Manager", null),
                CreateUser("Piotr", "Adamski", "piotr.adamski", "QA Specialist", 1),
                CreateUser("Ewa", "Zielińska", "ewa.zielinska", "Business Analyst", null),
                CreateUser("Tomasz", "Kwiatkowski", "tomasz.kwiatkowski", "DevOps Engineer", null),
                CreateUser("Magdalena", "Nowak", "magdalena.nowak", "Scrum Master", null),
                CreateUser("Krzysztof", "Wójcik", "krzysztof.wojcik", "Support Specialist", null),
                CreateUser("Natalia", "Lewandowska", "natalia.lewandowska", "Product Owner", null),

                // Użytkownicy bez przypisanego zespołu
                CreateUser("Paweł", "Dąbrowski", "pawel.dabrowski", "Junior Developer", null),
                CreateUser("Alicja", "Szymańska", "alicja.szymanska", "Data Scientist", null),
                CreateUser("Bartek", "Zieliński", "bartek.zielinski", "Cybersecurity Specialist", null),
                CreateUser("Monika", "Krawczyk", "monika.krawczyk", "Marketing Manager", null),
                CreateUser("Łukasz", "Kozłowski", "lukasz.kozlowski", "HR Specialist", null)
            };

            // Jeśli istnieje domyślny zespół, przypisujemy użytkownikom, którzy mają teamId != null, właściwy identyfikator
            var defaultTeam = await context.Teams.FirstOrDefaultAsync(t => t.Name == "Default Team");
            if (defaultTeam != null)
            {
                foreach (var user in users)
                {
                    if (user.TeamId != null)
                    {
                        user.TeamId = defaultTeam.Id;
                        user.Role = "TeamMember";
                    }
                    else
                    {
                        user.Role = "NonTeamMember";
                    }
                }
            }

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        private static User CreateUser(string name, string surname, string username, string position, int? teamId)
        {
            string password = "Password123";
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            return new User
            {
                Name = name,
                Surname = surname,
                Username = username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Position = position,
                TeamId = teamId,
                Role = teamId == null ? "NonTeamMember" : "TeamMember"
            };
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
