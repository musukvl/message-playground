using System.Data.SqlClient;
using System.Text;

namespace MessageTools.MessageLogger;

public class MessageDbCreator
{
    public static async Task CreateDbAndTables(string cstring, string databaseName)
    {
        using var sqlConnection = new SqlConnection(cstring);
        sqlConnection.InfoMessage +=  (object sender, SqlInfoMessageEventArgs e) => Console.WriteLine(e.Message);
        sqlConnection.Open();
        
        var ddl = File.ReadAllText("MessageLogger/ddl.sql");
        ddl = string.Format(ddl, databaseName);
        await using SqlCommand command = new SqlCommand(ddl, sqlConnection);
        await command.ExecuteNonQueryAsync();
        
    }
}