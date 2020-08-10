CREATE TABLE [dbo].[Link] (
    [ID]             INT            IDENTITY (1, 1) NOT NULL,
    [Name]           NVARCHAR (MAX) NULL,
    [URL]            NVARCHAR (MAX) NULL,
    [Description]    NVARCHAR (MAX) NULL,
    [LinkCategoryID] INT            NOT NULL,
    [Visible]        BIT            NULL,
    [Timestamp]      DATETIME       NULL,
    CONSTRAINT [PK_dbo.Link] PRIMARY KEY CLUSTERED ([ID] ASC)
);

