using AutoMapper;
using MPlayground.Contracts;

namespace MPlayground.WebApi.WebApiContract.User;

public class UpdateUserRequest
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }   
}

public class UpdateUserRequestProfile : Profile
{
    public UpdateUserRequestProfile()
    {
        CreateMap<UpdateUserRequest, UpdateUserMessage>();
    }
}