using System.Data;
using System.Data.SqlClient;
using MessageTools.Interceptor;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace MessageTools.MessageLogger;

public class MessageDbLogger : IMessageLogger
{
    private readonly SqlConnection _connection;
    
    public MessageDbLogger(string cstring)
    {
        _connection = new SqlConnection(cstring);
        _connection.Open();
    }
    
    const string INSERT_SQL = @"insert into MessageLog (MessageId, MessageType, ConversationId, MessageBody, ReceivedDate, MachineName, ProcessName, Assembly,MessageInfo) 
            values (@MessageId, @MessageType, @ConversationId, @MessageBody, @ReceivedDate, @MachineName, @ProcessName, @Assembly, @MessageInfo);";
    
    public async Task LogMassTransitMessage(MassTransitMessageBase messageBase, string messageBody, BasicDeliverEventArgs eventArgs)
    {
        await using SqlCommand command = new SqlCommand(INSERT_SQL, _connection);
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@MessageId", SqlDbType = SqlDbType.UniqueIdentifier,
            Value = messageBase.MessageId
        });
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@MessageType", SqlDbType = SqlDbType.NVarChar, Size = 255,
            Value = messageBase.GetMessageTypeShort()
        });
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@ConversationId", SqlDbType = SqlDbType.UniqueIdentifier,
            Value = messageBase.ConversationId
        });
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@MessageBody", SqlDbType = SqlDbType.NVarChar, Size = Int32.MaxValue,
            Value = messageBody
        });
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@ReceivedDate", SqlDbType = SqlDbType.DateTime,
            Value = DateTime.UtcNow
        });
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@MachineName", SqlDbType = SqlDbType.NVarChar, Size = 1024,
            Value = messageBase.Host.MachineName
        });
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@ProcessName", SqlDbType = SqlDbType.NVarChar, Size = 1024,
            Value = messageBase.Host.ProcessName
        });
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@Assembly", SqlDbType = SqlDbType.NVarChar, Size = 1024,
            Value = messageBase.Host.Assembly
        });
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@MessageInfo", SqlDbType = SqlDbType.NVarChar, Size = Int32.MaxValue,
            Value = JsonConvert.SerializeObject(new
            {
                Exchange = eventArgs.Exchange,
                RoutingKey = eventArgs.RoutingKey,
                BasicProperties = eventArgs.BasicProperties
            }, Formatting.Indented)
        });
        await command.PrepareAsync();
        await command.ExecuteNonQueryAsync();
    }

    public Task LogUnknownContentTypeMessage(string contentType, BasicDeliverEventArgs basicDeliverEventArgs1)
    {
        //Console.WriteLine($"[{basicDeliverEventArgs1..SentTime.ToString("yyyy-MM-dd HH:mm:ss")}]: {messageBase.GetMessageTypeShort()} from {messageBase.Host.ProcessName} {messageBase.ConversationId}");
        return Task.CompletedTask;
    }
    

    public void Dispose()
    {
        _connection.Close();
    }
}