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
            try
            {
                var toDoItem = await context.ToDoItems.FindAsync(id, cancellationToken);

                if (toDoItem is null || toDoItem.IsDeleted)
                {
                    Log.Error($"To Do Item with Id = {id} not found.");
                    throw new ArgumentException("To Do Item not found.");
                }

                var result = mapper.Map<ToDoItemResponseModel>(toDoItem);

                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get a To Do item: {ex.Message}");
                throw new Exception($"Failed to get a To Do item: {ex.Message}");
            }
        }

        public async Task<ToDoItemsListResponseModel> GetList(int pageSize, int pageNumber, ToDoItemStatus? toDoItemStatus, CancellationToken cancellationToken)
        {
            try
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
            catch (Exception ex)
            {
                Log.Error($"Failed to get a list of To Do items: {ex.Message}");
                throw new Exception($"Failed to get a list of To Do items: {ex.Message}");
            }
        }

        public async Task<ToDoItemResponseModel> Create(CreateToDoItemRequestModel requestModel, CancellationToken cancellationToken)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(requestModel);

                var newToDoItem = mapper.Map<ToDoItem>(requestModel);
                newToDoItem.Id = Guid.NewGuid();

                await context.ToDoItems.AddAsync(newToDoItem, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                return mapper.Map<ToDoItemResponseModel>(newToDoItem);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to create a To Do item: {ex.Message}");
                throw new Exception($"Failed to create a To Do item: {ex.Message}");
            }
        }

        public async Task<ToDoItemResponseModel> Update(Guid id, UpdateToDoItemRequestModel requestModel, CancellationToken cancellationToken)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(requestModel);

                var toDoItem = await context.ToDoItems.FindAsync(id, cancellationToken);

                if (toDoItem is null || toDoItem.IsDeleted)
                {
                    Log.Error($"To Do Item with Id = {id} not found.");
                    throw new ArgumentException("To Do Item not found.");
                }

                mapper.Map(requestModel, toDoItem);
                toDoItem.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync(cancellationToken);

                return mapper.Map<ToDoItemResponseModel>(toDoItem);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to update a To Do item: {ex.Message}");
                throw new Exception($"Failed to update a To Do item: {ex.Message}");
            }
        }

        public async Task<ToDoItemResponseModel> UpdateStatus(Guid id, ToDoItemStatus status, CancellationToken cancellationToken)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(status);

                var toDoItem = await context.ToDoItems.FindAsync(id, cancellationToken);

                if (toDoItem is null || toDoItem.IsDeleted)
                {
                    Log.Error($"To Do Item with Id = {id} not found.");
                    throw new ArgumentException("To Do Item not found.");
                }

                toDoItem.Status = status;
                toDoItem.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync(cancellationToken);

                return mapper.Map<ToDoItemResponseModel>(toDoItem);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to update sttatus: {ex.Message}");
                throw new Exception($"Failed to update sttatus: {ex.Message}");
            }
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var toDoItem = await context.ToDoItems.FindAsync(id, cancellationToken);

                if (toDoItem is null || toDoItem.IsDeleted)
                {
                    Log.Error($"To Do Item with Id = {id} not found.");
                    throw new ArgumentException("To Do Item not found.");
                }

                toDoItem.IsDeleted = true;

                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to delete a To Do item: {ex.Message}");
                throw new Exception($"Failed to delete a To Do item: {ex.Message}");
            }
        }

        public async Task DeleteAllCompleted(CancellationToken cancellationToken)
        {
            try
            {
                var toDoItems = await context.ToDoItems
                    .Where(x => x.Status == ToDoItemStatus.Compleated && !x.IsDeleted)
                    .ToListAsync(cancellationToken);

                toDoItems.ForEach(x => x.IsDeleted = true);

                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to delete completed To Do items: {ex.Message}");
                throw new Exception($"Failed to delete completed To Do items: {ex.Message}");
            }
        }
    }
}
