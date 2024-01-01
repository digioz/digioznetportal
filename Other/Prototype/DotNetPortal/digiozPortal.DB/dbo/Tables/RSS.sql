CREATE TABLE [dbo].[RSS] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [Name]      NVARCHAR (128) NOT NULL,
    [Url]       NVARCHAR (MAX) NOT NULL,
    [MaxCount]  INT            NOT NULL,
    [Timestamp] DATETIME       NOT NULL,
    CONSTRAINT [PK_dbo.RSS] PRIMARY KEY CLUSTERED ([ID] ASC)
);

