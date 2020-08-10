CREATE TABLE [dbo].[PollVote] (
    [Id]                UNIQUEIDENTIFIER NOT NULL,
    [PollAnswer_Id]     UNIQUEIDENTIFIER NOT NULL,
    [MembershipUser_Id] NVARCHAR (128)   NOT NULL,
    CONSTRAINT [PK_dbo.PollVote] PRIMARY KEY CLUSTERED ([Id] ASC)
);

