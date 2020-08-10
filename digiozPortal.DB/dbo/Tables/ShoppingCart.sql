CREATE TABLE [dbo].[ShoppingCart] (
    [ID]            NVARCHAR (128) NOT NULL,
    [UserID]        NVARCHAR (128) NOT NULL,
    [ProductID]     NVARCHAR (128) NOT NULL,
    [Quantity]      INT            NOT NULL,
    [DateCreated]   DATETIME       NOT NULL,
    [Size]          NVARCHAR (50)  NULL,
    [Color]         NVARCHAR (50)  NULL,
    [MaterialType]  NVARCHAR (50)  NULL,
    [Notes]         NVARCHAR (MAX) NULL,
    [AspNetUser_Id] NVARCHAR (128) NULL,
    CONSTRAINT [PK_dbo.ShoppingCart] PRIMARY KEY CLUSTERED ([ID] ASC)
);

