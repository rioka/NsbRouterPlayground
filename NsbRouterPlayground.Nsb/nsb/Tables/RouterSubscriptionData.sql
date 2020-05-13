CREATE TABLE [nsb].[RouterSubscriptionData] (
    [Subscriber]         NVARCHAR (200) NOT NULL,
    [Endpoint]           NVARCHAR (200) NULL,
    [MessageType]        NVARCHAR (200) NOT NULL,
    [PersistenceVersion] VARCHAR (23)   NOT NULL,
    PRIMARY KEY CLUSTERED ([Subscriber] ASC, [MessageType] ASC)
);

