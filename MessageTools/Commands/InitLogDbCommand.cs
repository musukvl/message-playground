using McMaster.Extensions.CommandLineUtils;
using MessageTools.MessageLogger;

namespace MessageTools.Commands;


[Command("init-log-db", Description = "Create/update log db in sql server")]
public class InitLogDbCommand 
{
    [Option("-c|--db-connection-string", "SQL Server Connection string", CommandOptionType.SingleValue)]
    public string DbConnectionString { get; set; }

    [Option("-db|--database-name", "Database name (Default: MessageLogDb)", CommandOptionType.SingleValue)]
    public string DatabaseName { get; set; } = "MessageLogDb";
    
    public async Task<int> OnExecute()
    {
        await MessageDbCreator.CreateDbAndTables(DbConnectionString, DatabaseName);
        return 0;
    }
}