using System.ComponentModel.DataAnnotations;
using ToDo.Domain.Enums;

namespace ToDo.Application.Common.Models
{
    public record CreateToDoItemRequestModel
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
    }
}
