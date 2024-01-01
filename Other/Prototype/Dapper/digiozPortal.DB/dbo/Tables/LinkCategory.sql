CREATE TABLE [dbo].[LinkCategory] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [Name]      NVARCHAR (MAX) NULL,
    [Visible]   BIT            NULL,
    [Timestamp] DATETIME       NULL,
    CONSTRAINT [PK_dbo.LinkCategory] PRIMARY KEY CLUSTERED ([ID] ASC)
);

