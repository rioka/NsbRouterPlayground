CREATE TABLE [nsb].[SubscriptionRouting] (
    [QueueAddress] NVARCHAR (200) NOT NULL,
    [Endpoint]     NVARCHAR (200) NOT NULL,
    [Topic]        NVARCHAR (200) NOT NULL,
    PRIMARY KEY CLUSTERED ([Endpoint] ASC, [Topic] ASC)
);

