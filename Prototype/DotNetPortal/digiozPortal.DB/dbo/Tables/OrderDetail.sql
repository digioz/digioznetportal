CREATE TABLE [dbo].[OrderDetail] (
    [ID]           NVARCHAR (128)  NOT NULL,
    [OrderID]      NVARCHAR (128)  NOT NULL,
    [ProductID]    NVARCHAR (128)  NOT NULL,
    [Quantity]     INT             NOT NULL,
    [UnitPrice]    DECIMAL (18, 2) NOT NULL,
    [Description]  NVARCHAR (MAX)  NULL,
    [Size]         NVARCHAR (50)   NULL,
    [Color]        NVARCHAR (50)   NULL,
    [MaterialType] NVARCHAR (50)   NULL,
    [Notes]        NVARCHAR (MAX)  NULL,
    CONSTRAINT [PK_dbo.OrderDetail] PRIMARY KEY CLUSTERED ([ID] ASC)
);

