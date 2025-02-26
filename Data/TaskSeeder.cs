using LicencjatUG.Server.Models;
using Microsoft.EntityFrameworkCore;
using TaskStatus = LicencjatUG.Server.Models.TaskStatus;

namespace LicencjatUG.Server.Data.Seeders
{
    public static class TaskSeeder
    {
        public static async Task SeedTasksAsync(DataContext context)
        {
            if (await context.Tasks.AnyAsync()) return;

            var userDictionary = context.Users.ToDictionary(u => u.Username);

            var tasks = new List<TaskEntity>
            {
                new TaskEntity
                    {
                        Title = "Zaprojektowanie UI",
                        Description = "Przygotowanie designu dla nowej strony logowania",
                        Status = TaskStatus.InProgress,
                        CreatedById = userDictionary["marek.wisniewski"].Id, // Project Manager
                        AssignedToId = userDictionary["katarzyna.lis"].Id, // UX Designer
                        CreatedDate = DateTime.UtcNow
                    },
                    new TaskEntity
                    {
                        Title = "Zaprojektowanie UI",
                        Description = "Przygotowanie designu dla nowych taskow",
                        Status = TaskStatus.InProgress,
                        CreatedById = userDictionary["marek.wisniewski"].Id, // Project Manager
                        AssignedToId = userDictionary["anna.kowalska"].Id, // UX Designer
                        CreatedDate = DateTime.UtcNow
                    },
                    new TaskEntity
                    {
                        Title = "Zaprojektowanie UI",
                        Description = "Projekt nowego UI dla dashboardu",
                        Status = TaskStatus.InProgress,
                        CreatedById = userDictionary["marek.wisniewski"].Id, // Project Manager
                        AssignedToId = userDictionary["anna.kowalska"].Id, // UX Designer
                        CreatedDate = DateTime.UtcNow
                    },
                    new TaskEntity
                    {
                        Title = "Zaprojektowanie UI",
                        Description = "Zaplanowanie widoku powiadomień",
                        Status = TaskStatus.InProgress,
                        CreatedById = userDictionary["marek.wisniewski"].Id, // Project Manager
                        AssignedToId = userDictionary["anna.kowalska"].Id, // UX Designer
                        CreatedDate = DateTime.UtcNow
                    },
                    new TaskEntity
                    {
                        Title = "Zaprojektowanie UI",
                        Description = "Projektowanie okna czatu grupowego",
                        Status = TaskStatus.InProgress,
                        CreatedById = userDictionary["marek.wisniewski"].Id, // Project Manager
                        AssignedToId = userDictionary["anna.kowalska"].Id, // UX Designer
                        CreatedDate = DateTime.UtcNow
                    },
                    new TaskEntity
                    {
                        Title = "Zaprojektowanie UI",
                        Description = "Makieta modalnego okna tworzenia nowej grupy czatowej",
                        Status = TaskStatus.InProgress,
                        CreatedById = userDictionary["marek.wisniewski"].Id, // Project Manager
                        AssignedToId = userDictionary["anna.kowalska"].Id, // UX Designer
                        CreatedDate = DateTime.UtcNow
                    },
                    new TaskEntity
                    {
                        Title = "Zaprojektowanie UI",
                        Description = "Stworzenie UI dla dodawania członków do zespołu",
                        Status = TaskStatus.InProgress,
                        CreatedById = userDictionary["marek.wisniewski"].Id, // Project Manager
                        AssignedToId = userDictionary["anna.kowalska"].Id, // UX Designer
                        CreatedDate = DateTime.UtcNow
                    },
                    new TaskEntity
                    {
                        Title = "Zaprojektowanie UI",
                        Description = "Projektowanie widoku szczegółów zespołu",
                        Status = TaskStatus.InProgress,
                        CreatedById = userDictionary["marek.wisniewski"].Id, // Project Manager
                        AssignedToId = userDictionary["anna.kowalska"].Id, // UX Designer
                        CreatedDate = DateTime.UtcNow
                    },
                    new TaskEntity
                    {
                        Title = "Zaprojektowanie UI",
                        Description = "Ulepszenie wyglądu listy zadań",
                        Status = TaskStatus.InProgress,
                        CreatedById = userDictionary["marek.wisniewski"].Id, // Project Manager
                        AssignedToId = userDictionary["anna.kowalska"].Id, // UX Designer
                        CreatedDate = DateTime.UtcNow
                    },
                    new TaskEntity
                    {
                        Title = "Zaprojektowanie UI",
                        Description = "Widok szczegółów zadania",
                        Status = TaskStatus.InProgress,
                        CreatedById = userDictionary["marek.wisniewski"].Id, // Project Manager
                        AssignedToId = userDictionary["anna.kowalska"].Id, // UX Designer
                        CreatedDate = DateTime.UtcNow
                    },
                    new TaskEntity
                    {
                        Title = "Zaprojektowanie UI",
                        Description = "Projektowanie kalendarza zespołowego",
                        Status = TaskStatus.InProgress,
                        CreatedById = userDictionary["marek.wisniewski"].Id, // Project Manager
                        AssignedToId = userDictionary["anna.kowalska"].Id, // UX Designer
                        CreatedDate = DateTime.UtcNow
                    },
                    new TaskEntity
                    {
                        Title = "Implementacja API logowania",
                        Description = "Stworzenie API do logowania użytkowników",
                        Status = TaskStatus.InProgress,
                        CreatedById = userDictionary["marek.wisniewski"].Id,
                        AssignedToId = userDictionary["jan.nowak"].Id, // Backend Developer
                        CreatedDate = DateTime.UtcNow
                    },
                    new TaskEntity
                    {
                        Title = "Testy jednostkowe API",
                        Description = "Dodanie testów dla funkcji logowania i rejestracji",
                        Status = TaskStatus.Issue,
                        CreatedById = userDictionary["marek.wisniewski"].Id,
                        AssignedToId = userDictionary["piotr.adamski"].Id, // QA Specialist
                        CreatedDate = DateTime.UtcNow
                    },
                    new TaskEntity
                    {
                        Title = "Optymalizacja aplikacji",
                        Description = "Poprawienie wydajności oraz analiza zapytań SQL",
                        Status = TaskStatus.Done,
                        CreatedById = userDictionary["marek.wisniewski"].Id,
                        AssignedToId = userDictionary["jan.nowak"].Id,
                        CreatedDate = DateTime.UtcNow
                    }
            };

            await context.Tasks.AddRangeAsync(tasks);
            await context.SaveChangesAsync();
        }
    }
}
