using System.ComponentModel.DataAnnotations;
using ToDo.Domain.Enums;

namespace ToDo.Application.Common.Models
{
    public record UpdateToDoItemStatusRequestModel
    {
        [Range(0, 1)]
        public ToDoItemStatus Status { get; set; } = ToDoItemStatus.Active;
    }
}
