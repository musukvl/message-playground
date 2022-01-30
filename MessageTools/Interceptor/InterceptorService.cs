using System.Collections.Concurrent;
using System.Text;
using MessageTools.MessageLogger;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageTools.Interceptor;

public class InterceptorService
{
    private readonly IInterceptorSettings _interceptorSettings;

    public InterceptorService(IInterceptorSettings interceptorSettings)
    {
        _interceptorSettings = interceptorSettings;
    }

    private readonly ConcurrentDictionary<Guid, byte> _processedMessages = new ();

    public void ConfigureAndStartConsumer()
    {
        var messageLoggers = CreateMessageLoggers();
        
        
        var factory = new ConnectionFactory() { HostName = _interceptorSettings.HostName, UserName = _interceptorSettings.User, Password = _interceptorSettings.Password };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        ConfigureRabbitMqChannel(channel);
        
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += OnConsumerOnReceived;
        
        async void OnConsumerOnReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            try
            {
                await ConsumeMessage(basicDeliverEventArgs, messageLoggers);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        
        channel.BasicConsume(queue: "interceptor-queue",
            autoAck: true,
            consumer: consumer);
    }

    private List<IMessageLogger> CreateMessageLoggers()
    {
        var messageLoggers = new List<IMessageLogger>();
        if (!string.IsNullOrEmpty(_interceptorSettings.DbConnectionString))
        {
            using var dbLogger = new MessageDbLogger(_interceptorSettings.DbConnectionString);
            messageLoggers.Add(dbLogger);
        }

        if (!_interceptorSettings.NoConsoleLogging)
        {
            messageLoggers.Add(new ConsoleLogger());
        }
        return messageLoggers;
    }

    async Task ConsumeMessage(BasicDeliverEventArgs basicDeliverEventArgs1, IReadOnlyCollection<IMessageLogger> messageDbLogger)
    {
        var contentType = GetContentType(basicDeliverEventArgs1);
        if (contentType == "application/vnd.masstransit+json")
        {
            await ConsumeMassTransitMessage(basicDeliverEventArgs1, messageDbLogger);
        }
        else
        {
            await ConsumeUnknownContentTypeMessage(basicDeliverEventArgs1, messageDbLogger, contentType);
        }
    }

    private static async Task ConsumeUnknownContentTypeMessage(BasicDeliverEventArgs basicDeliverEventArgs1, IReadOnlyCollection<IMessageLogger> messageDbLogger, string contentType)
    {
        foreach (var logger in messageDbLogger)
        {
            await logger.LogUnknownContentTypeMessage(contentType, basicDeliverEventArgs1);
        }
    }

    private async Task ConsumeMassTransitMessage(BasicDeliverEventArgs basicDeliverEventArgs1, IReadOnlyCollection<IMessageLogger> messageDbLogger)
    {
        var massTransitMessageBody = Encoding.UTF8.GetString(basicDeliverEventArgs1.Body.ToArray());
        var messageBase = JsonConvert.DeserializeObject<MassTransitMessageBase>(massTransitMessageBody);
        if (_processedMessages.ContainsKey(messageBase.MessageId))
            return;
        _processedMessages.TryAdd(messageBase.MessageId, 0);
        
        var messageType = messageBase.GetMessageTypeShort();
        if (IsMessageTypeIgnored(messageType, _interceptorSettings.IgnoredMessageTypes))
            return;

        foreach (var logger in messageDbLogger)
        {
            await logger.LogMassTransitMessage(messageBase, massTransitMessageBody, basicDeliverEventArgs1);
        }
    }

    private bool IsMessageTypeIgnored(string messageType, string[] interceptorSettingsIgnoredMessageTypes)
    {
        return interceptorSettingsIgnoredMessageTypes.Contains(messageType);
    }

    string GetContentType(BasicDeliverEventArgs basicDeliverEventArgs)
    {
        if (basicDeliverEventArgs.BasicProperties.Headers.ContainsKey("properties"))
        {
            if (basicDeliverEventArgs.BasicProperties.Headers["properties"] is IDictionary<string, object> properties)
            {
                if (properties["content_type"] is byte[] contentTypeBytes)
                {
                    return Encoding.UTF8.GetString(contentTypeBytes);
                }
            }
        }
        return String.Empty;
    }

    void ConfigureRabbitMqChannel(IModel model)
    {
        model.ExchangeDeclare(exchange: "interceptor-exchange", type: ExchangeType.Topic);
        var queue = model.QueueDeclare(queue: "interceptor-queue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        var queueName = queue.QueueName;
        model.QueueBind(queue: queueName,
            exchange: "amq.rabbitmq.trace",
            routingKey: "#");
    }
}