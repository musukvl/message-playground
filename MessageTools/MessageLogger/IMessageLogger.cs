using MessageTools.Interceptor;
using RabbitMQ.Client.Events;

namespace MessageTools.MessageLogger;

public interface IMessageLogger : IDisposable
{
    Task LogMassTransitMessage(MassTransitMessageBase messageBase, string messageBody, BasicDeliverEventArgs eventArgs);
    Task LogUnknownContentTypeMessage(string contentType, BasicDeliverEventArgs basicDeliverEventArgs1);
}