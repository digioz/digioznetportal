CREATE TABLE [dbo].[MailingListCampaign] (
    [ID]           UNIQUEIDENTIFIER NOT NULL,
    [Name]         NVARCHAR (255)   NOT NULL,
    [Subject]      NVARCHAR (255)   NOT NULL,
    [FromName]     NVARCHAR (50)    NOT NULL,
    [FromEmail]    NVARCHAR (50)    NOT NULL,
    [Summary]      NVARCHAR (255)   NOT NULL,
    [Banner]       NVARCHAR (255)   NULL,
    [Body]         NVARCHAR (MAX)   NOT NULL,
    [VisitorCount] INT              NOT NULL,
    [DateCreated]  DATETIME         NOT NULL,
    CONSTRAINT [PK_dbo.MailingListCampaign] PRIMARY KEY CLUSTERED ([ID] ASC)
);

