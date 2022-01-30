using McMaster.Extensions.CommandLineUtils;

namespace MessageTools.Commands;

public abstract class RabbitMqCommand : CommandBase
{
    [Option("-h|--host", "RabbitMQ Host (default: localhost)", CommandOptionType.SingleValue)]
    public string HostName { get; set; } = "localhost";

    [Option("-u|--user", "RabbitMQ User (default: guest)", CommandOptionType.SingleValue)]
    public string User { get; set; } = "guest";

    [Option("-p|--password", "RabbitMQ Password (default: guest)", CommandOptionType.SingleValue)]
    public string Password { get; set; } = "guest";
}