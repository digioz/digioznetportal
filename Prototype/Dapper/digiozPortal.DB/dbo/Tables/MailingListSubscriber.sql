CREATE TABLE [dbo].[MailingListSubscriber] (
    [ID]           UNIQUEIDENTIFIER NOT NULL,
    [Email]        NVARCHAR (50)    NOT NULL,
    [FirstName]    NVARCHAR (50)    NULL,
    [LastName]     NVARCHAR (50)    NULL,
    [Status]       BIT              NOT NULL,
    [DateCreated]  DATETIME         NOT NULL,
    [DateModified] DATETIME         NOT NULL,
    CONSTRAINT [PK_dbo.MailingListSubscriber] PRIMARY KEY CLUSTERED ([ID] ASC)
);

