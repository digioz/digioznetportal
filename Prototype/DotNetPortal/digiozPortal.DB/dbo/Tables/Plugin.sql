CREATE TABLE [dbo].[Plugin] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Name]      NVARCHAR (MAX) NOT NULL,
    [DLL]       NVARCHAR (MAX) NULL,
    [IsEnabled] BIT            NOT NULL,
    CONSTRAINT [PK_dbo.Plugin] PRIMARY KEY CLUSTERED ([Id] ASC)
);

