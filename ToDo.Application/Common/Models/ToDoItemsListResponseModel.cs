namespace ToDo.Application.Common.Models
{
    public record ToDoItemsListResponseModel
    {
        public int TotalCount { get; set; }
        public int CurrentPageCount => ToDoItems.Count;
        public required List<ToDoItemResponseModel> ToDoItems { get; set; }
    }
}
