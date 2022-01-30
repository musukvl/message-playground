using System.Collections.Concurrent;
using System.Text;
using MessageTools.MessageLogger;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageTools.Interceptor;

public class Interceptor : IDisposable
{
    private readonly IInterceptorSettings _interceptorSettings;
    public  List<IMessageLogger> MessageLoggers { get; set; }
    public  IConnection RabbitMqClientConnection { get; set; }
    public IModel Channel { get; set; }

    public readonly ConcurrentDictionary<Guid, byte> ProcessedMessages = new ();

    public Interceptor(IInterceptorSettings interceptorSettings)
    {
        _interceptorSettings = interceptorSettings;
    }
    
    public async Task ConsumeMessage(BasicDeliverEventArgs basicDeliverEventArgs1)
    {
        var contentType = GetContentType(basicDeliverEventArgs1);
        if (contentType == "application/vnd.masstransit+json")
        {
            await ConsumeMassTransitMessage(basicDeliverEventArgs1);
        }
        else
        {
            await ConsumeUnknownContentTypeMessage(basicDeliverEventArgs1,  contentType);
        }
    }

    private async Task ConsumeUnknownContentTypeMessage(BasicDeliverEventArgs basicDeliverEventArgs1, string contentType)
    {
        foreach (var logger in MessageLoggers)
        {
            await logger.LogUnknownContentTypeMessage(contentType, basicDeliverEventArgs1);
        }
    }

    private async Task ConsumeMassTransitMessage(BasicDeliverEventArgs basicDeliverEventArgs1)
    {
        var massTransitMessageBody = Encoding.UTF8.GetString(basicDeliverEventArgs1.Body.ToArray());
        var messageBase = JsonConvert.DeserializeObject<MassTransitMessageBase>(massTransitMessageBody);
        if (ProcessedMessages.ContainsKey(messageBase.MessageId))
            return;
        ProcessedMessages.TryAdd(messageBase.MessageId, 0);
        
        var messageType = messageBase.GetMessageTypeShort();
        if (IsMessageTypeIgnored(messageType, _interceptorSettings.IgnoredMessageTypes))
            return;

        foreach (var logger in MessageLoggers)
        {
            await logger.LogMassTransitMessage(messageBase, massTransitMessageBody, basicDeliverEventArgs1);
        }
    }

    private bool IsMessageTypeIgnored(string messageType, string[] interceptorSettingsIgnoredMessageTypes)
    {
        if (interceptorSettingsIgnoredMessageTypes == null)
            return false;
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
    
    public void Close()
    {
        Channel.Close();
        RabbitMqClientConnection.Close();
    }
    
    public void Dispose()
    {
        Close();
        foreach (var messageLogger in MessageLoggers)
        {
            messageLogger.Dispose();
        }
        
        Channel.Dispose();
      
        RabbitMqClientConnection.Dispose();
    }
    
}