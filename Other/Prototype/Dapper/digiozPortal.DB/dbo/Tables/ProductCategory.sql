CREATE TABLE [dbo].[ProductCategory] (
    [ID]          NVARCHAR (128) NOT NULL,
    [Name]        NVARCHAR (100) NOT NULL,
    [Visible]     BIT            NOT NULL,
    [Description] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.ProductCategory] PRIMARY KEY CLUSTERED ([ID] ASC)
);

