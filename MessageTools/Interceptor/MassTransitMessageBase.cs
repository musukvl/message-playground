namespace MessageTools.Interceptor;

public class MassTransitMessageBase
{
    public string[] MessageType { get; set; }
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }

    public Host Host { get; set; }
    
    public DateTime SentTime { get; set; }
    
    public string GetMessageTypeShort()
    {
        var messageType = MessageType.First();
        var result =  messageType.Split(":").Where((item, index) => index > 2) ;
        return string.Join(":", result);
    }
}

public class Host
{
    public string MachineName { get; set; }
    public string ProcessName { get; set; }
    public string Assembly { get; set; }
}