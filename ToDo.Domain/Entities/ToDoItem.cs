using System.ComponentModel.DataAnnotations;
using ToDo.Domain.Enums;

namespace ToDo.Domain.Entities
{
    public class ToDoItem
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime DateOfCreation { get; set; } = DateTime.Now;
        [Range(0, 1)]
        public ToDoItemStatus Status { get; set; } = ToDoItemStatus.Active;
        public bool IsDeleted { get; set; }
    }
}
