CREATE TABLE [dbo].[Announcement] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [UserID]    NVARCHAR (128) NULL,
    [Title]     NVARCHAR (MAX) NULL,
    [Body]      NVARCHAR (MAX) NULL,
    [Visible]   BIT            NOT NULL,
    [Timestamp] DATETIME       NULL,
    CONSTRAINT [PK_dbo.Announcement] PRIMARY KEY CLUSTERED ([ID] ASC)
);

