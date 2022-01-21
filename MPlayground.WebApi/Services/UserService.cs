using AutoMapper;
using MassTransit;
using MPlayground.Contracts;
using MPlayground.WebApi.WebApiContract.User;

namespace MPlayground.WebApi.Services;

public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;

    public UserService(ILogger<UserService> logger, IPublishEndpoint publishEndpoint, IMapper mapper)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
    }
    
    public async Task UpdateUser(UpdateUserRequest updateUserRequest)
    {
        var message = _mapper.Map<UpdateUserMessage>(updateUserRequest);
        await _publishEndpoint.Publish(message);
    }
}