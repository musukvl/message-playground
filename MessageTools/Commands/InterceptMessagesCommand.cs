using McMaster.Extensions.CommandLineUtils;
using MessageTools.Interceptor; 

namespace MessageTools.Commands;

[Command("intercept-messages", Description = "Listen messages from queue")]
public class InterceptMessageCommand : RabbitMqCommand, IInterceptorSettings
{
    [Option("-c|--db-connection-string", "SQL Server Connection string", CommandOptionType.SingleValue)]
    public string DbConnectionString { get; set; } 

    [Option("-nc|--no-console-logging", "No message logging to console", CommandOptionType.SingleValue)]
    public bool NoConsoleLogging { get; set; }

    [Option("-im|--ignored-messages", "Ignored messages", CommandOptionType.MultipleValue)]
    public string[] IgnoredMessageTypes { get; set; }

    public override async Task<int> OnExecute()
    { 
        var interceptorService = new MessageInterceptorFactory();
        using (interceptorService.StartInterceptorConsumer(this))
        {
            Console.WriteLine("Press any key to stop interception.");
            Console.ReadLine();
            return 0;
        }
    }
}