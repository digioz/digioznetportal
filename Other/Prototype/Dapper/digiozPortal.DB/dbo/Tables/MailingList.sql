CREATE TABLE [dbo].[MailingList] (
    [ID]               UNIQUEIDENTIFIER NOT NULL,
    [Name]             NVARCHAR (50)    NOT NULL,
    [DefaultEmailFrom] NVARCHAR (50)    NOT NULL,
    [DefaultFromName]  NVARCHAR (50)    NOT NULL,
    [Description]      NVARCHAR (MAX)   NOT NULL,
    [Address]          NVARCHAR (MAX)   NOT NULL,
    CONSTRAINT [PK_dbo.MailingList] PRIMARY KEY CLUSTERED ([ID] ASC)
);

