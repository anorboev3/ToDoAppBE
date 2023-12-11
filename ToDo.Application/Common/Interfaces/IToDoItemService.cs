using ToDo.Application.Common.Models;
using ToDo.Domain.Enums;

namespace ToDo.Application.Common.Interfaces
{
    public interface IToDoItemService
    {
        public Task<ToDoItemResponseModel> Get(Guid id, CancellationToken cancellationToken);
        public Task<ToDoItemsListResponseModel> GetList(int pageSize, int pageNumber, ToDoItemStatus? toDoItemStatus, CancellationToken cancellationToken);
        public Task<ToDoItemResponseModel> Create(CreateToDoItemRequestModel requestModel, CancellationToken cancellationToken);
        public Task<ToDoItemResponseModel> Update(Guid id, UpdateToDoItemRequestModel requestModel, CancellationToken cancellationToken);
        public Task<ToDoItemResponseModel> UpdateStatus(Guid id, ToDoItemStatus status, CancellationToken cancellationToken);
        public Task Delete(Guid id, CancellationToken cancellationToken);
        public Task DeleteAllCompleted(CancellationToken cancellationToken);
    }
}
