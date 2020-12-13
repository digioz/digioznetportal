CREATE TABLE [dbo].[PollUsersVote] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [UserId]    NVARCHAR (MAX) NULL,
    [PollId]    NVARCHAR (MAX) NULL,
    [DateVoted] DATETIME       NOT NULL,
    CONSTRAINT [PK_dbo.PollUsersVote] PRIMARY KEY CLUSTERED ([Id] ASC)
);

