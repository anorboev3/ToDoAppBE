using AutoMapper;
using ToDo.Application.Common.Models;
using ToDo.Domain.Entities;

namespace ToDo.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ToDoItem, ToDoItemResponseModel>();
            CreateMap<CreateToDoItemRequestModel, ToDoItem>();
            CreateMap<UpdateToDoItemRequestModel, ToDoItem>();
        }
    }
}
