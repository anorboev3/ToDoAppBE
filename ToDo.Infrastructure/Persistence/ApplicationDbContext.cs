using Microsoft.EntityFrameworkCore;
using ToDo.Application.Common.Interfaces;
using ToDo.Domain.Entities;

namespace ToDo.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public required DbSet<ToDoItem> ToDoItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var folder = Path.Combine(Environment.CurrentDirectory, "DB");
                Directory.CreateDirectory(folder);
                var pathToDbFile = Path.Combine(folder, "ToDo.db");
                optionsBuilder.UseSqlite($"Data Source={pathToDbFile}");
            }
        }
    }
}
