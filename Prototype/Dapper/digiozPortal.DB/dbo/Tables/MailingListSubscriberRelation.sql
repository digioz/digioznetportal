CREATE TABLE [dbo].[MailingListSubscriberRelation] (
    [ID]                      UNIQUEIDENTIFIER NOT NULL,
    [MailingListID]           UNIQUEIDENTIFIER NOT NULL,
    [MailingListSubscriberID] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_dbo.MailingListSubscriberRelation] PRIMARY KEY CLUSTERED ([ID] ASC)
);

