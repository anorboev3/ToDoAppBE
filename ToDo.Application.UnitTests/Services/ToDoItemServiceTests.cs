using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using Moq.EntityFrameworkCore;
using ToDo.Application.Common.Interfaces;
using ToDo.Application.Common.Models;
using ToDo.Application.Services;
using ToDo.Domain.Entities;
using ToDo.Domain.Enums;

namespace ToDo.Application.UnitTests.Services
{
    [TestFixture]
    public class ToDoItemServiceTest
    {
        private Mock<IApplicationDbContext> _mockContext;
        private Mock<IMapper> _mockMapper;
        private ToDoItemService _service;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _mockMapper = new Mock<IMapper>();

            _service = new ToDoItemService(_mockContext.Object, _mockMapper.Object);
        }

        [Test]
        public async Task GetToDoItem_ReturnsToDoItemResponseModel_WhenToDoItemExists()
        {
            var toDoItemId = Guid.NewGuid();
            var toDoIyem = GenerateToDoItem(toDoItemId);
            var toDoItemResponseModel = GenerateToDoItemResponseModel(toDoItemId, toDoIyem.Title, toDoIyem.Description, toDoIyem.DateOfCreation, toDoIyem.Status);
            _mockContext.Setup(ctx => ctx.ToDoItems.FindAsync(toDoItemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(toDoIyem);
            _mockMapper.Setup(m => m.Map<ToDoItemResponseModel>(It.IsAny<ToDoItem>()))
                .Returns(toDoItemResponseModel);

            var result = await _service.Get(toDoItemId, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(toDoItemResponseModel));
        }

        [Test]
        public void GetToDoItem_ThrowsAnException_WhenToDoItemNotExists()
        {
            var toDoItemId = Guid.NewGuid();
            _mockContext.Setup(ctx => ctx.ToDoItems.FindAsync(Guid.NewGuid(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ToDoItem)null);

            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.Get(toDoItemId, CancellationToken.None));

            Assert.That(ex.Message, Is.EqualTo($"Get To Do Item: To Do Item with Id = {toDoItemId} not found."));
        }

        [Test]
        public async Task GetList_ReturnsFullPageOfToDoItems_WhenPageSizeIsLessThenTotalCount()
        {
            var mockToDoItems = GenerateDoItemsList();
            _mockContext.Setup(ctx => ctx.ToDoItems)
                        .ReturnsDbSet(mockToDoItems);
            _mockMapper.Setup(m => m.Map<ToDoItemResponseModel>(It.IsAny<ToDoItem>()))
               .Returns((ToDoItem source) => new ToDoItemResponseModel
               {
                   Id = source.Id,
                   Title = source.Title,
                   DateOfCreation = source.DateOfCreation,
                   Description = source.Description,
                   IsDeleted = source.IsDeleted,
                   Status = source.Status,
                   UpdatedAt = source.UpdatedAt
               });

            int pageSize = 5;
            int pageNumber = 1;
            ToDoItemStatus? status = null;
            var filteredMockToDoItems = mockToDoItems.Where(x => !x.IsDeleted).OrderByDescending(x => x.DateOfCreation).ToList();
            var expectedResult = new ToDoItemsListResponseModel
            {
                ToDoItems = filteredMockToDoItems
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => _mockMapper.Object.Map<ToDoItemResponseModel>(x))
                    .ToList(),
                TotalCount = filteredMockToDoItems.Count
            };

            var result = await _service.GetList(pageSize, pageNumber, status, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ToDoItemsListResponseModel>());
            Assert.That(result.CurrentPageCount, Is.EqualTo(expectedResult.CurrentPageCount));
            Assert.That(result.TotalCount, Is.EqualTo(expectedResult.TotalCount));
            Assert.That(result.ToDoItems, Is.EqualTo(expectedResult.ToDoItems));
        }
        [Test]
        public async Task GetList_ReturnsNotFullPageToDoItems_WhenPageNumberIsLastPosiblePage()
        {
            var mockToDoItems = GenerateDoItemsList();
            _mockContext.Setup(ctx => ctx.ToDoItems)
                        .ReturnsDbSet(mockToDoItems);
            _mockMapper.Setup(m => m.Map<ToDoItemResponseModel>(It.IsAny<ToDoItem>()))
               .Returns((ToDoItem source) => new ToDoItemResponseModel
               {
                   Id = source.Id,
                   Title = source.Title,
                   DateOfCreation = source.DateOfCreation,
                   Description = source.Description,
                   IsDeleted = source.IsDeleted,
                   Status = source.Status,
                   UpdatedAt = source.UpdatedAt
               });

            
            int pageSize = 7;
            int pageNumber = 2;
            ToDoItemStatus? status = null;
            var filteredMockToDoItems = mockToDoItems.Where(x => !x.IsDeleted).OrderByDescending(x => x.DateOfCreation).ToList();
            var expectedResult = new ToDoItemsListResponseModel
            {
                ToDoItems = filteredMockToDoItems
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => _mockMapper.Object.Map<ToDoItemResponseModel>(x))
                    .ToList(),
                TotalCount = filteredMockToDoItems.Count
            };

            var result = await _service.GetList(pageSize, pageNumber, status, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ToDoItemsListResponseModel>());
            Assert.That(result.CurrentPageCount, Is.EqualTo(expectedResult.CurrentPageCount));
            Assert.That(result.TotalCount, Is.EqualTo(expectedResult.TotalCount));
            Assert.That(result.ToDoItems, Is.EqualTo(expectedResult.ToDoItems));
        }

        [Test]
        [TestCase(ToDoItemStatus.Active)]
        [TestCase(ToDoItemStatus.Completed)]
        public async Task GetList_ReturnsOnlyActiveToDoItem_WhenActiveStatusPassedToMethod(ToDoItemStatus status)
        {
            var mockToDoItems = GenerateDoItemsList();
            _mockContext.Setup(ctx => ctx.ToDoItems)
                        .ReturnsDbSet(mockToDoItems);
            _mockMapper.Setup(m => m.Map<ToDoItemResponseModel>(It.IsAny<ToDoItem>()))
               .Returns((ToDoItem source) => new ToDoItemResponseModel
               {
                   Id = source.Id,
                   Title = source.Title,
                   DateOfCreation = source.DateOfCreation,
                   Description = source.Description,
                   IsDeleted = source.IsDeleted,
                   Status = source.Status,
                   UpdatedAt = source.UpdatedAt
               });


            int pageSize = 5;
            int pageNumber = 1;
            var filteredMockToDoItems = mockToDoItems.Where(x => !x.IsDeleted && x.Status == status).ToList();
            var expectedResult = new ToDoItemsListResponseModel
            {
                ToDoItems = filteredMockToDoItems
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => _mockMapper.Object.Map<ToDoItemResponseModel>(x))
                    .ToList(),
                TotalCount = filteredMockToDoItems.Count
            };

            var result = await _service.GetList(pageSize, pageNumber, status, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ToDoItemsListResponseModel>());
            result.ToDoItems.ForEach(x =>
            {
                Assert.That(x.Status, Is.EqualTo(status));
            });
        }

        [Test]
        public void Create_ShouldThrowArgumentNullException_WhenRequestModelIsNull()
        {
            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.Create(null, CancellationToken.None)
            );
        }

        [Test]
        public async Task Create_ShouldReturnToDoItemResponseModel_WhenValidRequestModelIsProvided()
        {
            var toDoItemId = Guid.NewGuid();
            var requestModel = GenerateCreateToDoItemRequestModel();
            var toDoItem = GenerateToDoItem(toDoItemId);
            var responseModel = GenerateToDoItemResponseModel(toDoItemId, requestModel.Title, requestModel.Description, toDoItem.DateOfCreation, ToDoItemStatus.Active);

            _mockMapper.Setup(m => m.Map<ToDoItem>(requestModel))
                .Returns(toDoItem);
            _mockContext.Setup(ctx => ctx.ToDoItems.AddAsync(It.IsAny<ToDoItem>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<EntityEntry<ToDoItem>>());
            _mockMapper.Setup(m => m.Map<ToDoItemResponseModel>(It.IsAny<ToDoItem>()))
                .Returns(responseModel);

            var result = await _service.Create(requestModel, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ToDoItemResponseModel>());
            Assert.That(result, Is.EqualTo(responseModel));
            _mockContext.Verify(ctx => ctx.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Update_ReturnsUpdatedToDoItemResponseModel_WhenCalled()
        {
            var toDoItemId = Guid.NewGuid();
            var toDoItem = GenerateToDoItem(toDoItemId);
            var requestModel = GenerateUpdateToDoItemRequestModel(ToDoItemStatus.Completed);
            var responseModel = GenerateToDoItemResponseModel(toDoItemId, requestModel.Title, requestModel.Description, toDoItem.DateOfCreation, requestModel.Status);

            _mockContext.Setup(ctx => ctx.ToDoItems.FindAsync(toDoItemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(toDoItem);
            _mockMapper.Setup(m => m.Map(It.IsAny<UpdateToDoItemRequestModel>(), It.IsAny<ToDoItem>()))
               .Callback<UpdateToDoItemRequestModel, ToDoItem>((src, dest) =>
               {
                   dest.Title = src.Title;
                   dest.Description = src.Description;
                   dest.Status = src.Status;
               });
            _mockMapper.Setup(m => m.Map<ToDoItemResponseModel>(It.IsAny<ToDoItem>()))
               .Returns((ToDoItem src) => new ToDoItemResponseModel
               {
                   Id = src.Id,
                   Title = src.Title,
                   Description = src.Description,
                   Status = src.Status,
                   DateOfCreation = src.DateOfCreation,
                   IsDeleted = src.IsDeleted,
               });
            _mockContext.Setup(ctx => ctx.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _service.Update(toDoItemId, requestModel, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ToDoItemResponseModel>());
            Assert.That(result, Is.EqualTo(responseModel));
            _mockContext.Verify(ctx => ctx.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Update_ThrowsArgumentException_WhenToDoItemDoesNotExist()
        {
            var toDoItemId = Guid.NewGuid();
            var requestModel = GenerateUpdateToDoItemRequestModel(ToDoItemStatus.Completed);

            _mockContext.Setup(ctx => ctx.ToDoItems.FindAsync(toDoItemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ToDoItem)null);

            var updateAction = async () => await _service.Update(toDoItemId, requestModel, CancellationToken.None);

            Assert.That(updateAction, Throws.TypeOf<ArgumentException>()
                .With.Message.EqualTo($"Update To Do Item: To Do Item with Id = {toDoItemId} not found."));
        }

        [Test]
        public void Update_ShouldThrowArgumentNullException_WhenRequestModelIsNull()
        {
            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.Update(Guid.NewGuid(), null, CancellationToken.None)
            );
        }

        [Test]
        public async Task UpdateStatus_ReturnsUpdatedToDoItemResponseModel_WhenCalled()
        {
            var toDoItemId = Guid.NewGuid();
            var toDoItem = GenerateToDoItem(toDoItemId);
            var statusToUpdate = new UpdateToDoItemStatusRequestModel { Status = ToDoItemStatus.Completed };
            var responseModel = GenerateToDoItemResponseModel(toDoItemId, toDoItem.Title, toDoItem.Description, toDoItem.DateOfCreation, statusToUpdate.Status);

            _mockContext.Setup(ctx => ctx.ToDoItems.FindAsync(toDoItemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(toDoItem);
            _mockMapper.Setup(m => m.Map<ToDoItemResponseModel>(It.IsAny<ToDoItem>()))
               .Returns((ToDoItem src) => new ToDoItemResponseModel
               {
                   Id = src.Id,
                   Title = src.Title,
                   Description = src.Description,
                   Status = src.Status,
                   DateOfCreation = src.DateOfCreation,
                   IsDeleted = src.IsDeleted,
               });
            _mockContext.Setup(ctx => ctx.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _service.UpdateStatus(toDoItemId, statusToUpdate, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ToDoItemResponseModel>());
            Assert.That(result, Is.EqualTo(responseModel));
            _mockContext.Verify(ctx => ctx.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void UpdateStatus_ThrowsArgumentException_WhenToDoItemDoesNotExist()
        {
            var toDoItemId = Guid.NewGuid();

            _mockContext.Setup(ctx => ctx.ToDoItems.FindAsync(toDoItemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ToDoItem)null);

            var updateStatusAction = async () => await _service.UpdateStatus(toDoItemId, new UpdateToDoItemStatusRequestModel { Status = ToDoItemStatus.Completed }, CancellationToken.None);

            Assert.That(updateStatusAction, Throws.TypeOf<ArgumentException>()
                .With.Message.EqualTo($"Update To Do Item Status: To Do Item with Id = {toDoItemId} not found."));
        }

        [Test]
        public async Task Delete_MarksToDoItemAsDeleted_WhenToDoItemExists()
        {
            var toDoItemId = Guid.NewGuid();
            var toDoItem = GenerateToDoItem(toDoItemId);
            _mockContext.Setup(ctx => ctx.ToDoItems.FindAsync(toDoItemId, CancellationToken.None))
                .ReturnsAsync(toDoItem);
            _mockContext.Setup(ctx => ctx.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(1);

            await _service.Delete(toDoItemId, CancellationToken.None);

            Assert.That(toDoItem.IsDeleted, Is.True);
            _mockContext.Verify(ctx => ctx.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        [Test]
        public void Delete_ThrowsArgumentException_WhenToDoItemDoesNotExist()
        {
            var mockToDoItems = GenerateDoItemsList();
            var toDoItemId = Guid.NewGuid();

            _mockContext.Setup(ctx => ctx.ToDoItems)
                        .ReturnsDbSet(mockToDoItems);

            var deleteAction = async () => await _service.Delete(toDoItemId, CancellationToken.None);

            Assert.That(deleteAction, Throws.TypeOf<ArgumentException>()
                .With.Message.EqualTo($"Delete To Do Item: To Do Item with Id = {toDoItemId} not found."));
        }

        [Test]
        public async Task DeleteAllCompleted_MarksAllCompletedToDoItemsAsDeleted()
        {
            var toDoItemsList = GenerateDoItemsList();
            _mockContext.Setup(ctx => ctx.ToDoItems).ReturnsDbSet(toDoItemsList);
            _mockContext.Setup(ctx => ctx.SaveChangesAsync(CancellationToken.None)).ReturnsAsync(toDoItemsList.Count);

            await _service.DeleteAllCompleted(CancellationToken.None);

            toDoItemsList
                .Where(x => x.Status == ToDoItemStatus.Completed)
                .ToList()
                .ForEach(x =>
                    {
                        Assert.That(x.IsDeleted, Is.True);
                    });
            _mockContext.Verify(ctx => ctx.SaveChangesAsync(CancellationToken.None), Times.Once);
        }

        private static ToDoItemResponseModel GenerateToDoItemResponseModel(Guid id, string title, string description, DateTime dateOfCreation, ToDoItemStatus status)
        {
            return new ToDoItemResponseModel
            {
                Id = id,
                Title = title,
                DateOfCreation = dateOfCreation,
                Description = description,
                IsDeleted = false,
                Status = status
            };
        }

        private static CreateToDoItemRequestModel GenerateCreateToDoItemRequestModel()
        {
            return new CreateToDoItemRequestModel
            {
                Title = "ToDoItem 1 Title",
                Description = "ToDoItem 3 Description"
            };
        }

        private static UpdateToDoItemRequestModel GenerateUpdateToDoItemRequestModel(ToDoItemStatus status)
        {
            return new UpdateToDoItemRequestModel
            {
                Title = "Updated ToDoItem 1 Title",
                Description = "Updated ToDoItem 3 Description",
                Status = status
            };
        }

        private static ToDoItem GenerateToDoItem(Guid id)
        {
            return new ToDoItem
            {
                Id = id,
                Title = "ToDoItem 1 Title",
                DateOfCreation = DateTime.Now,
                Description = "ToDoItem 1 Description",
                IsDeleted = false,
                Status = ToDoItemStatus.Active
            };
        }

        private static ToDoItem GenerateDoItem(Guid id, string title, string description, ToDoItemStatus status)
        {
            return new ToDoItem
            {
                Id = id,
                Title = title,
                DateOfCreation = DateTime.Now,
                Description = description,
                IsDeleted = false,
                Status = status
            };
        }

        private static List<ToDoItem> GenerateDoItemsList()
        {
            return new List<ToDoItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "ToDoItem 1 Title",
                    DateOfCreation = DateTime.Now,
                    Description = "ToDoItem 1 Description",
                    IsDeleted = false,
                    Status = ToDoItemStatus.Active
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "ToDoItem 2 Title",
                    DateOfCreation = DateTime.Now,
                    Description = "ToDoItem 2 Description",
                    IsDeleted = false,
                    Status = ToDoItemStatus.Active
                },
                new() {
                    Id = Guid.NewGuid(),
                    Title = "ToDoItem 3 Title",
                    DateOfCreation = DateTime.Now,
                    Description = "ToDoItem 3 Description",
                    IsDeleted = false,
                    Status = ToDoItemStatus.Active
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "ToDoItem 4 Title",
                    DateOfCreation = DateTime.Now,
                    Description = "ToDoItem 4 Description",
                    IsDeleted = false,
                    Status = ToDoItemStatus.Active
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "ToDoItem 5 Title",
                    DateOfCreation = DateTime.Now,
                    Description = "ToDoItem 5 Description",
                    IsDeleted = false,
                    Status = ToDoItemStatus.Active
                },
                new() {
                    Id = Guid.NewGuid(),
                    Title = "ToDoItem 6 Title",
                    DateOfCreation = DateTime.Now,
                    Description = "ToDoItem 6 Description",
                    IsDeleted = false,
                    Status = ToDoItemStatus.Active
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "ToDoItem 7 Title",
                    DateOfCreation = DateTime.Now,
                    Description = "ToDoItem 7 Description",
                    IsDeleted = false,
                    Status = ToDoItemStatus.Active
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "ToDoItem 8 Title",
                    DateOfCreation = DateTime.Now,
                    Description = "ToDoItem 8 Description",
                    IsDeleted = false,
                    Status = ToDoItemStatus.Completed
                },
                new() {
                    Id = Guid.NewGuid(),
                    Title = "ToDoItem 9 Title",
                    DateOfCreation = DateTime.Now,
                    Description = "ToDoItem 9 Description",
                    IsDeleted = false,
                    Status = ToDoItemStatus.Completed
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "ToDoItem 10 Title",
                    DateOfCreation = DateTime.Now,
                    Description = "ToDoItem 10 Description",
                    IsDeleted = false,
                    Status = ToDoItemStatus.Completed
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "ToDoItem 11 Title",
                    DateOfCreation = DateTime.Now,
                    Description = "ToDoItem 11 Description",
                    IsDeleted = true,
                    Status = ToDoItemStatus.Completed
                }
            };
        }
    }
}
