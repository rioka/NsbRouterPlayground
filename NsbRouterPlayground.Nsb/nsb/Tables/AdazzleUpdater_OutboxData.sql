CREATE TABLE [nsb].[AdazzleUpdater_OutboxData] (
    [MessageId]          NVARCHAR (200) NOT NULL,
    [Dispatched]         BIT            DEFAULT ((0)) NOT NULL,
    [DispatchedAt]       DATETIME       NULL,
    [PersistenceVersion] VARCHAR (23)   NOT NULL,
    [Operations]         NVARCHAR (MAX) NOT NULL,
    PRIMARY KEY NONCLUSTERED ([MessageId] ASC)
);


GO
CREATE NONCLUSTERED INDEX [Index_DispatchedAt]
    ON [nsb].[AdazzleUpdater_OutboxData]([DispatchedAt] ASC) WHERE ([Dispatched]=(1));

