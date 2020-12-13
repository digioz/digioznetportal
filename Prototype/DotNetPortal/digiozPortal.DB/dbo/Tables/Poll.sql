CREATE TABLE [dbo].[Poll] (
    [Id]                       UNIQUEIDENTIFIER NOT NULL,
    [Slug]                     NVARCHAR (MAX)   NULL,
    [IsClosed]                 BIT              NOT NULL,
    [DateCreated]              DATETIME         NOT NULL,
    [Featured]                 BIT              NOT NULL,
    [AllowMultipleOptionsVote] BIT              NOT NULL,
    [MembershipUser_Id]        NVARCHAR (128)   NOT NULL,
    CONSTRAINT [PK_dbo.Poll] PRIMARY KEY CLUSTERED ([Id] ASC)
);

