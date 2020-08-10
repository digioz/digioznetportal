CREATE TABLE [dbo].[VideoAlbum] (
    [ID]          BIGINT         IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (MAX) NOT NULL,
    [Description] NVARCHAR (MAX) NOT NULL,
    [Timestamp]   DATETIME       NULL,
    [Visible]     BIT            NULL,
    CONSTRAINT [PK_dbo.VideoAlbum] PRIMARY KEY CLUSTERED ([ID] ASC)
);

