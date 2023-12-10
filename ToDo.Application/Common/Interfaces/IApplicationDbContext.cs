using Microsoft.EntityFrameworkCore;
using ToDo.Domain.Entities;

namespace ToDo.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<ToDoItem> ToDoItems { get; set; }
    }
}
