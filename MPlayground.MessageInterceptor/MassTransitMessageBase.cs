namespace MPlayground.MessageInterceptor;

public class MassTransitMessageBase
{
    public string[] MessageType { get; set; }
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }

    public string GetMessageTypeShort()
    {
        var messageType = MessageType.Last();
        return messageType.Split(":").Last();
    }
}