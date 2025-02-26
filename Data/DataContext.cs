using LicencjatUG.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace LicencjatUG.Server.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Team> Teams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 🔹 Relacja: Team -> Owner (User)
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Owner)               // Team ma jednego Owner
                .WithMany(u => u.OwnedTeams)        // Owner może być właścicielem wielu Teamów
                .HasForeignKey(t => t.OwnerId)      // Klucz obcy w Team
                .OnDelete(DeleteBehavior.Restrict); // Ogranicz usuwanie, aby uniknąć kaskadowego usuwania

            // 🔹 Relacja: User -> Team (jeden użytkownik ma jeden zespół)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Team)               // User ma jednego Team
                .WithMany(t => t.Members)           // Team ma wielu Members
                .HasForeignKey(u => u.TeamId)       // Klucz obcy w User
                .OnDelete(DeleteBehavior.Restrict); // Ogranicz usuwanie, aby uniknąć kaskadowego usuwania

            // ✅ Poprawiona relacja: Task -> CreatedBy User
            modelBuilder.Entity<TaskEntity>()
                .HasOne(t => t.CreatedBy)
                .WithMany() // ❗ Musimy dodać `.WithMany()`, aby EF Core wiedział, że User ma wiele TaskEntity
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.NoAction); // ✅ Brak akcji usunięcia → unika cyklu

            // ✅ Poprawiona relacja: Task -> AssignedTo User
            modelBuilder.Entity<TaskEntity>()
                .HasOne(t => t.AssignedTo)
                .WithMany() // ❗ Musimy dodać `.WithMany()`
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull); // ✅ Jeśli użytkownik zostanie usunięty, AssignedToId = NULL

            base.OnModelCreating(modelBuilder);
        }
    }
}
