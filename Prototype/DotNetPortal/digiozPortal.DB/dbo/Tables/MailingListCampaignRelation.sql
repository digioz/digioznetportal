CREATE TABLE [dbo].[MailingListCampaignRelation] (
    [ID]                    UNIQUEIDENTIFIER NOT NULL,
    [MailingListID]         UNIQUEIDENTIFIER NOT NULL,
    [MailingListCampaignID] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_dbo.MailingListCampaignRelation] PRIMARY KEY CLUSTERED ([ID] ASC)
);

