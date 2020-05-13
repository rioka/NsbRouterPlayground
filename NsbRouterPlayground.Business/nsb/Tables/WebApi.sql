﻿CREATE TABLE [nsb].[WebApi] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [CorrelationId]  VARCHAR (255)    NULL,
    [ReplyToAddress] VARCHAR (255)    NULL,
    [Recoverable]    BIT              NOT NULL,
    [Expires]        DATETIME         NULL,
    [Headers]        NVARCHAR (MAX)   NOT NULL,
    [Body]           VARBINARY (MAX)  NULL,
    [RowVersion]     BIGINT           IDENTITY (1, 1) NOT NULL
);


GO
CREATE CLUSTERED INDEX [Index_RowVersion]
    ON [nsb].[WebApi]([RowVersion] ASC);


GO
CREATE NONCLUSTERED INDEX [Index_Expires]
    ON [nsb].[WebApi]([Expires] ASC)
    INCLUDE([Id], [RowVersion]) WHERE ([Expires] IS NOT NULL);

