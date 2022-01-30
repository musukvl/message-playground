using System.Text;
using MessageTools.MessageLogger;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageTools.Interceptor;

public class MessageInterceptorFactory
{
    public Interceptor StartInterceptorConsumer(IInterceptorSettings interceptorSettings)
    {
        var interceptor = new Interceptor(interceptorSettings);
        interceptor.MessageLoggers  = CreateMessageLoggers(interceptorSettings);
        
        var factory = new ConnectionFactory() { HostName = interceptorSettings.HostName, UserName = interceptorSettings.User, Password = interceptorSettings.Password };
        interceptor.RabbitMqClientConnection = factory.CreateConnection();
        interceptor.Channel = interceptor.RabbitMqClientConnection.CreateModel();
        
        ConfigureRabbitMqChannel(interceptor.Channel);
        
        var consumer = new EventingBasicConsumer(interceptor.Channel);
        consumer.Received += async (object sender, BasicDeliverEventArgs basicDeliverEventArgs) =>
        {
            try
            {
                await interceptor.ConsumeMessage(basicDeliverEventArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        };
        
        interceptor.Channel.BasicConsume(queue: "interceptor-queue",
            autoAck: true,
            consumer: consumer);
        return interceptor;
    }

    private List<IMessageLogger> CreateMessageLoggers(IInterceptorSettings interceptorSettings)
    {
        var messageLoggers = new List<IMessageLogger>();
        if (!string.IsNullOrEmpty(interceptorSettings.DbConnectionString))
        {
            var dbLogger = new MessageDbLogger(interceptorSettings.DbConnectionString);
            messageLoggers.Add(dbLogger);
        }

        if (!interceptorSettings.NoConsoleLogging)
        {
            messageLoggers.Add(new ConsoleLogger());
        }
        return messageLoggers;
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