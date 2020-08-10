CREATE TABLE [dbo].[CommentConfig] (
    [Id]             NVARCHAR (128) NOT NULL,
    [ReferenceId]    NVARCHAR (MAX) NULL,
    [ReferenceType]  NVARCHAR (MAX) NULL,
    [ReferenceTitle] NVARCHAR (MAX) NULL,
    [Visible]        BIT            NOT NULL,
    [Timestamp]      DATETIME       NOT NULL,
    CONSTRAINT [PK_dbo.CommentConfig] PRIMARY KEY CLUSTERED ([Id] ASC)
);

