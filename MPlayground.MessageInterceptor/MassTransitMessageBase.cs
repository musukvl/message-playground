namespace MPlayground.MessageInterceptor;

public class MassTransitMessageBase
{
    public string[] MessageType { get; set; }
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }

    public Host Host { get; set; }
    
    public DateTime SentTime { get; set; }
    
    public string GetMessageTypeShort()
    {
        var messageType = MessageType.Last();
        return messageType.Split(":").Last();
    }
}

public class Host
{
    public string MachineName { get; set; }
    public string ProcessName { get; set; }
    public string Assembly { get; set; }
}