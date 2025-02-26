using LicencjatUG.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace LicencjatUG.Server.Data.Seeders
{
    public static class TeamSeeder
    {
        public static async Task SeedTeamsAsync(DataContext context)
        {
            if (await context.Teams.AnyAsync()) return;

            User owner;
            if (!await context.Users.AnyAsync())
            {
                // Tworzymy domyślnego właściciela, jeśli nie ma żadnych użytkowników
                owner = new User
                {
                    Name = "Default",
                    Surname = "Owner",
                    Username = "default.owner",
                    // W tym przykładzie używamy pustych tablic – w praktyce możesz ustawić hasło lub skorzystać z metody hashującej
                    PasswordHash = Array.Empty<byte>(),
                    PasswordSalt = Array.Empty<byte>(),
                    Position = "Owner",
                    Role = "TeamMember",
                    TeamId = null
                };
                await context.Users.AddAsync(owner);
                await context.SaveChangesAsync();
            }
            else
            {
                owner = await context.Users.FirstAsync();
            }

            var team = new Team
            {
                Name = "Default Team",
                OwnerId = owner.Id // Ustawiamy właściciela zespołu
            };

            await context.Teams.AddAsync(team);
            await context.SaveChangesAsync();

            // Jeśli właściciel nie miał przypisanego zespołu, aktualizujemy jego rekord
            if (owner.TeamId == null)
            {
                owner.TeamId = team.Id;
                context.Users.Update(owner);
                await context.SaveChangesAsync();
            }
        }
    }
}
