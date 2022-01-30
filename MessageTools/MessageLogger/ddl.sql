use master;
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{0}')

BEGIN
    CREATE DATABASE {0};
    print 'Database {0} created';
END


USE {0};

if exists (select * from sysobjects where name='MessageLog' and xtype='U')
begin
  Drop TABLE MessageLog;
  print 'Table MessageLog dropped';
END

CREATE TABLE MessageLog (
    MessageId UNIQUEIDENTIFIER Primary Key,
    MessageType NVARCHAR(255),
    ReceivedDate DATETIME,
    MachineName NVARCHAR(1024),
    Assembly NVARCHAR(1024),
    ProcessName NVARCHAR(1024),
    ConversationId UNIQUEIDENTIFIER,
    MessageInfo NVARCHAR(MAX),
    MessageBody NVARCHAR(MAX)    
);

create index MessageLog_ReceivedDate_index
    on MessageLog (ReceivedDate desc);

print 'Table MessageLog created';


 