using MessageTools.Interceptor;
using RabbitMQ.Client.Events;

namespace MessageTools.MessageLogger;

public class ConsoleLogger : IMessageLogger
{
    public Task LogMassTransitMessage(MassTransitMessageBase messageBase, string messageBody, BasicDeliverEventArgs eventArgs)
    {
        Console.WriteLine($"[{messageBase.SentTime.ToString("yyyy-MM-dd HH:mm:ss")}]: {messageBase.GetMessageTypeShort()} from {messageBase.Host.ProcessName} {messageBase.ConversationId}");
        return Task.CompletedTask;
    }

    public Task LogUnknownContentTypeMessage(string contentType, BasicDeliverEventArgs basicDeliverEventArgs1)
    {
        //Console.WriteLine($"[{basicDeliverEventArgs1..SentTime.ToString("yyyy-MM-dd HH:mm:ss")}]: {messageBase.GetMessageTypeShort()} from {messageBase.Host.ProcessName} {messageBase.ConversationId}");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}