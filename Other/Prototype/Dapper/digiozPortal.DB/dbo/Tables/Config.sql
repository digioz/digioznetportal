CREATE TABLE [dbo].[Config] (
    [ID]          INT            IDENTITY (1, 1) NOT NULL,
    [ConfigKey]   NVARCHAR (MAX) NOT NULL,
    [ConfigValue] NVARCHAR (MAX) NOT NULL,
    [IsEncrypted] BIT            NOT NULL,
    CONSTRAINT [PK_dbo.Config] PRIMARY KEY CLUSTERED ([ID] ASC)
);

