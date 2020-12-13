CREATE TABLE [dbo].[Zone] (
    [ID]       INT            IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (MAX) NOT NULL,
    [ZoneType] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_dbo.Zone] PRIMARY KEY CLUSTERED ([ID] ASC)
);

