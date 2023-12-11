using System.ComponentModel.DataAnnotations;
using ToDo.Domain.Enums;

namespace ToDo.Application.Common.Models
{
    public record UpdateToDoItemRequestModel
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        [Range(0, 1)]
        public ToDoItemStatus Status { get; set; } = ToDoItemStatus.Active;
    }
}
