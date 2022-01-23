// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using System.Text;
using MPlayground.MessageInterceptor;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


var factory = new ConnectionFactory() { HostName = "localhost", UserName = "QueueUser", Password = "QueueUser00"};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

ConfigureRabbitMqChannel(channel);

using var dbLogger = new MessageDbLogger("Server=localhost,5433;Database=MessageLogDb;User=sa;Password=SqlUser00;TrustServerCertificate=true;MultipleActiveResultSets=true");
//await dbLogger.EnsureDbTable();

var consumer = new EventingBasicConsumer(channel);

var receivedMessages = new ConcurrentBag<Guid>();
consumer.Received += async (sender, basicDeliverEventArgs) =>
{
    try
    {
        await ConsumeMessage(basicDeliverEventArgs, receivedMessages, dbLogger);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
};

channel.BasicConsume(queue: "interceptor-queue",
    autoAck: true,
    consumer: consumer);

Console.WriteLine("Subscribed to the queue 'interceptor-queue'");
Console.ReadLine();

///-----------------------------------------------------------------

async Task ConsumeMessage(BasicDeliverEventArgs basicDeliverEventArgs1, ConcurrentBag<Guid> concurrentBag, MessageDbLogger messageDbLogger)
{
    var contentType = GetContentType(basicDeliverEventArgs1);
    if (contentType == "application/vnd.masstransit+json")
    {
        var messageBody = Encoding.UTF8.GetString(basicDeliverEventArgs1.Body.ToArray());
        var messageBase = JsonConvert.DeserializeObject<MassTransitMessageBase>(messageBody);
        if (concurrentBag.Contains(messageBase.MessageId))
            return;
        concurrentBag.Add(messageBase.MessageId);
        await messageDbLogger.LogMessage(messageBase, messageBody, basicDeliverEventArgs1);
        
        Console.WriteLine($" Intercepted message: {messageBase.GetMessageTypeShort()} {messageBase.MessageId}");
    }
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