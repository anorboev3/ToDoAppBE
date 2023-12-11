using System.ComponentModel.DataAnnotations;
using ToDo.Domain.Enums;

namespace ToDo.Application.Common.Models
{
    public record ToDoItemResponseModel
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime DateOfCreation { get; set; } = DateTime.Now;
        [Range(0, 1)]
        public ToDoItemStatus Status { get; set; } = ToDoItemStatus.Active;
        public bool IsDeleted { get; set; }
    }
}
