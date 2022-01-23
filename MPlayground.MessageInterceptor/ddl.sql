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
    ConversationId UNIQUEIDENTIFIER,
    MessageBody NVARCHAR(MAX),
    ReceivedDate DATETIME
)

 