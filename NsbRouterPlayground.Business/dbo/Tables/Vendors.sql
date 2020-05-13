CREATE TABLE [dbo].[Vendors] (
    [Id]   INT              IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (100)   NOT NULL,
    [Uid]  UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Vendors] PRIMARY KEY CLUSTERED ([Id] ASC)
);

