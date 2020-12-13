CREATE TABLE [dbo].[CommentLike] (
    [Id]        NVARCHAR (128) NOT NULL,
    [UserId]    NVARCHAR (MAX) NULL,
    [CommentId] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.CommentLike] PRIMARY KEY CLUSTERED ([Id] ASC)
);

