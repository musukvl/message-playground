using MassTransit;
using MPlayground.Contracts;
using Newtonsoft.Json;

namespace MPlaygroud.UserService.Consumers;

public class UpdateUserConsumer : IConsumer<UpdateUserMessage>
{
    private readonly ILogger<UpdateUserConsumer> _logger;

    public UpdateUserConsumer(ILogger<UpdateUserConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UpdateUserMessage> context)
    {
        var message = JsonConvert.SerializeObject(context.Message, Formatting.Indented);
        _logger.LogInformation("User update event consumed successfully: {Message}", message);
    }
}