using System.Data;
using System.Data.SqlClient;

namespace MPlayground.MessageInterceptor;

public class MessageDbLogger : IDisposable
{
    private readonly SqlConnection _connection;
    
    public MessageDbLogger(string cstring)
    {
        _connection = new SqlConnection(cstring);
        _connection.Open();
    }

    public async Task EnsureDbTable()
    {
        var ddl = File.ReadAllText("ddl.sql");
        await using SqlCommand command = new SqlCommand(ddl, _connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task LogMessage(MassTransitMessageBase messageBase, string messageBody)
    {
        var sql = @"insert into MessageLog (MessageId, MessageType, ConversationId, MessageBody, ReceivedDate) values (@MessageId, @MessageType, @ConversationId, @MessageBody, @ReceivedDate);";
             
        await using SqlCommand command = new SqlCommand(sql, _connection);
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@MessageId", SqlDbType = SqlDbType.UniqueIdentifier,
            Value = messageBase.MessageId
        });
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@MessageType", SqlDbType = SqlDbType.NVarChar,
            Size = 255,
            Value = messageBase.GetMessageTypeShort()
        });
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@ConversationId", SqlDbType = SqlDbType.UniqueIdentifier,
            Value = messageBase.ConversationId
        });
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@MessageBody", SqlDbType = SqlDbType.NVarChar,
            Size = Int32.MaxValue,
            Value = messageBody
        });
        command.Parameters.Add(new SqlParameter()
        {
            ParameterName = "@ReceivedDate", SqlDbType = SqlDbType.DateTime,
            Value = DateTime.UtcNow
        });
        await command.PrepareAsync();
        await command.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        _connection.Close();
    }
}