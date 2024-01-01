CREATE TABLE [dbo].[Comment] (
    [Id]            NVARCHAR (128) NOT NULL,
    [ParentId]      NVARCHAR (MAX) NULL,
    [UserId]        NVARCHAR (MAX) NULL,
    [Username]      NVARCHAR (MAX) NULL,
    [ReferenceId]   NVARCHAR (MAX) NULL,
    [ReferenceType] NVARCHAR (MAX) NULL,
    [Body]          NVARCHAR (MAX) NULL,
    [CreatedDate]   DATETIME       NOT NULL,
    [ModifiedDate]  DATETIME       NOT NULL,
    [Likes]         INT            NOT NULL,
    CONSTRAINT [PK_dbo.Comment] PRIMARY KEY CLUSTERED ([Id] ASC)
);

