CREATE TABLE [dbo].[Page] (
    [ID]          INT            IDENTITY (1, 1) NOT NULL,
    [UserID]      NVARCHAR (128) NULL,
    [Title]       NVARCHAR (MAX) NULL,
    [URL]         NVARCHAR (MAX) NULL,
    [Body]        NVARCHAR (MAX) NULL,
    [Keywords]    NVARCHAR (MAX) NULL,
    [Description] NVARCHAR (MAX) NULL,
    [Visible]     BIT            NOT NULL,
    [Timestamp]   DATETIME       NULL,
    CONSTRAINT [PK_dbo.Page] PRIMARY KEY CLUSTERED ([ID] ASC)
);

