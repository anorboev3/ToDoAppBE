using Microsoft.AspNetCore.Mvc;
using ToDo.Application.Common.Interfaces;
using ToDo.Application.Common.Models;
using ToDo.Domain.Enums;

namespace ToDo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoItemController(IToDoItemService toDoItemService) : Controller
    {
        /// <summary>
        /// Takes a single To Do item by Id.
        /// </summary>
        /// <param name="id">Id of To Do item to get.</param>
        /// <returns>A To Do item.</returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await toDoItemService.Get(id, cancellationToken));
        }

        /// <summary>
        /// Takes a paginated list of To Do items, optionally filtered by status.
        /// </summary>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="pageNumber">Page number, starting from 1.</param>
        /// <param name="toDoItemStatus">Optional filter for To Do item status (Active = 0, Completed = 1).</param>
        /// <returns>A paginated list of To Do items according to the specified parameters.</returns>
        [HttpGet]
        public async Task<IActionResult> GetList(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] ToDoItemStatus? toDoItemStatus,
            CancellationToken cancellationToke)
        {
            return Ok(await toDoItemService.GetList(pageSize, pageNumber, toDoItemStatus, cancellationToke));
        }

        /// <summary>
        /// Creates a new To Do items.
        /// </summary>
        /// <param name="toDoItemDto">To Do Item object to create.</param>
        /// <returns>A new created To Do item.</returns>
        [HttpPost]
        public async Task<IActionResult> Create(CreateToDoItemRequestModel toDoItemDto, CancellationToken cancellationToken)
        {
            return Ok(await toDoItemService.Create(toDoItemDto, cancellationToken));
        }

        /// <summary>
        /// Updates the To Do item by Id.
        /// </summary>
        /// <param name="id">Id of To Do item to update.</param>
        /// <param name="toDoItemDto">To Do item object for updating.</param>
        /// <returns>An updated To Do item.</returns>
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateToDoItemRequestModel toDoItemDto, CancellationToken cancellationToken)
        {
            return Ok(await toDoItemService.Update(id, toDoItemDto, cancellationToken));
        }

        /// <summary>
        /// Updates the status of the To Do item by Id.
        /// </summary>
        /// <param name="id">Id of To Do item to update status.</param>
        /// <param name="status">Status for updating (Active = 0, Completed = 1).</param>
        /// <returns>A To Do item with updated status.</returns>
        [HttpPut]
        [Route("update-status/{id}")]
        public async Task<IActionResult> UpdateStatus(Guid id, ToDoItemStatus status, CancellationToken cancellationToken)
        {
            return Ok(await toDoItemService.UpdateStatus(id, status, cancellationToken));
        }

        /// <summary>
        /// Deletes a single To Do item by Id.
        /// </summary>
        /// <param name="id">Id of To Do item to delete.</param>
        /// <returns>If successful returns Status200OK, otherwise Status400BadRequest.</returns>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await toDoItemService.Delete(id, cancellationToken);
            return Ok();
        }

        /// <summary>
        /// Deletes all To Do items with status "Completed"(1).
        /// </summary>
        /// <param name="id">Id of To Do item to delete.</param>
        /// <returns>If successful returns Status200OK, otherwise Status400BadRequest.</returns>
        [HttpDelete]
        [Route("delete-all-completed")]
        public async Task<IActionResult> DeleteAllCompleted(CancellationToken cancellationToken)
        {
            await toDoItemService.DeleteAllCompleted(cancellationToken);
            return Ok();
        }
    }
}
