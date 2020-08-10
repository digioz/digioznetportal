CREATE TABLE [dbo].[Picture] (
    [ID]          BIGINT         IDENTITY (1, 1) NOT NULL,
    [UserID]      NVARCHAR (128) NULL,
    [AlbumID]     BIGINT         NULL,
    [Filename]    NVARCHAR (MAX) NULL,
    [Description] NVARCHAR (MAX) NULL,
    [Timestamp]   DATETIME       NULL,
    [Approved]    BIT            NULL,
    [Visible]     BIT            NULL,
    CONSTRAINT [PK_dbo.Picture] PRIMARY KEY CLUSTERED ([ID] ASC)
);

