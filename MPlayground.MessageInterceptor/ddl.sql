use master;
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'MessageLogDb')

BEGIN
    CREATE DATABASE MessageLogDb
END


USE MessageLogDb

if exists (select * from sysobjects where name='MessageLog' and xtype='U')
begin
Drop TABLE MessageLog
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
)

create index MessageLog_ReceivedDate_index
    on MessageLog (ReceivedDate desc)
    go



 