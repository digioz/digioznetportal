CREATE TABLE [dbo].[Video] (
    [ID]          BIGINT         IDENTITY (1, 1) NOT NULL,
    [UserID]      NVARCHAR (128) NULL,
    [AlbumID]     BIGINT         NULL,
    [Filename]    NVARCHAR (MAX) NULL,
    [Description] NVARCHAR (MAX) NULL,
    [Timestamp]   DATETIME       NULL,
    [Approved]    BIT            NULL,
    [Visible]     BIT            NULL,
    [Thumbnail]   NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.Video] PRIMARY KEY CLUSTERED ([ID] ASC)
);

