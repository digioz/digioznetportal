CREATE TABLE [dbo].[Log] (
    [ID]        BIGINT         IDENTITY (1, 1) NOT NULL,
    [Message]   NVARCHAR (MAX) NULL,
    [Timestamp] DATETIME       NULL,
    CONSTRAINT [PK_dbo.Log] PRIMARY KEY CLUSTERED ([ID] ASC)
);

