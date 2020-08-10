CREATE TABLE [dbo].[Chat] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [UserID]    NVARCHAR (128) NULL,
    [Message]   NVARCHAR (MAX) NULL,
    [Timestamp] DATETIME       NOT NULL,
    CONSTRAINT [PK_dbo.Chat] PRIMARY KEY CLUSTERED ([ID] ASC)
);

