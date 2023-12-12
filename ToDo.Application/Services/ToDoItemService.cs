using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ToDo.Application.Common.Interfaces;
using ToDo.Application.Common.Models;
using ToDo.Domain.Entities;
using ToDo.Domain.Enums;

namespace ToDo.Application.Services
{
    public class ToDoItemService(IApplicationDbContext context, IMapper mapper) : IToDoItemService
    {
        public async Task<ToDoItemResponseModel> Get(Guid id, CancellationToken cancellationToken)
        {
            var toDoItem = await context.ToDoItems.FindAsync(id, cancellationToken);

            if (toDoItem is null || toDoItem.IsDeleted)
            {
                var errorMessage = $"Get To Do Item: To Do Item with Id = {id} not found.";
                Log.Error(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            var result = mapper.Map<ToDoItemResponseModel>(toDoItem);

            return result;
        }

        public async Task<ToDoItemsListResponseModel> GetList(int pageSize, int pageNumber, ToDoItemStatus? toDoItemStatus, CancellationToken cancellationToken)
        {
            var query = toDoItemStatus is null
                ? context.ToDoItems.Where(x => !x.IsDeleted)
                : context.ToDoItems.Where(x => !x.IsDeleted && x.Status == toDoItemStatus);

            var toDoItems = await query
                .Where(x => !x.IsDeleted)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => mapper.Map<ToDoItemResponseModel>(x))
                .ToListAsync(cancellationToken);

            return new ToDoItemsListResponseModel { ToDoItems = toDoItems, TotalCount = query.Count() };
        }

        public async Task<ToDoItemResponseModel> Create(CreateToDoItemRequestModel requestModel, CancellationToken cancellationToken)
        {
            if (requestModel is null)
            {
                var errorMessage = $"Create To Do Item:The input argument {nameof(requestModel)} can not be null.";
                Log.Error(errorMessage);
                throw new ArgumentNullException(nameof(requestModel), errorMessage);
            }

            ArgumentNullException.ThrowIfNull(requestModel);

            var newToDoItem = mapper.Map<ToDoItem>(requestModel);
            newToDoItem.Id = Guid.NewGuid();

            await context.ToDoItems.AddAsync(newToDoItem, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return mapper.Map<ToDoItemResponseModel>(newToDoItem);
        }

        public async Task<ToDoItemResponseModel> Update(Guid id, UpdateToDoItemRequestModel requestModel, CancellationToken cancellationToken)
        {
            if (requestModel is null)
            {
                var errorMessage = $"Update To Do Item: The input argument {nameof(requestModel)} can not be null.";
                Log.Error(errorMessage);
                throw new ArgumentNullException(nameof(requestModel), errorMessage);
            }

            var toDoItem = await context.ToDoItems.FindAsync(id, cancellationToken);

            if (toDoItem is null || toDoItem.IsDeleted)
            {
                var errorMessage = $"Update To Do Item: To Do Item with Id = {id} not found.";
                Log.Error(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            mapper.Map(requestModel, toDoItem);
            toDoItem.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return mapper.Map<ToDoItemResponseModel>(toDoItem);
        }

        public async Task<ToDoItemResponseModel> UpdateStatus(Guid id, ToDoItemStatus status, CancellationToken cancellationToken)
        {
            var toDoItem = await context.ToDoItems.FindAsync(id, cancellationToken);

            if (toDoItem is null || toDoItem.IsDeleted)
            {
                var errorMessage = $"Update To Do Item Status: To Do Item with Id = {id} not found.";
                Log.Error(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            toDoItem.Status = status;
            toDoItem.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return mapper.Map<ToDoItemResponseModel>(toDoItem);
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            var toDoItem = await context.ToDoItems.FindAsync(id, cancellationToken);

            if (toDoItem is null || toDoItem.IsDeleted)
            {
                var errorMessage = $"Delete To Do Item: To Do Item with Id = {id} not found.";
                Log.Error(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            toDoItem.IsDeleted = true;

            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAllCompleted(CancellationToken cancellationToken)
        {
            var toDoItems = await context.ToDoItems
                    .Where(x => x.Status == ToDoItemStatus.Completed && !x.IsDeleted)
                    .ToListAsync(cancellationToken);

            toDoItems.ForEach(x => x.IsDeleted = true);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
