CREATE TABLE [dbo].[ProductOption] (
    [ID]          NVARCHAR (128) NOT NULL,
    [ProductID]   NVARCHAR (128) NOT NULL,
    [OptionType]  NVARCHAR (50)  NOT NULL,
    [OptionValue] NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_dbo.ProductOption] PRIMARY KEY CLUSTERED ([ID] ASC)
);

