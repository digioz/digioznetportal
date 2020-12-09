CREATE TABLE [dbo].[SlideShow] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Image]       NVARCHAR (MAX) NULL,
    [Description] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.SlideShow] PRIMARY KEY CLUSTERED ([Id] ASC)
);

