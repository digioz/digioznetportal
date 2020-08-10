CREATE TABLE [dbo].[LogVisitor] (
    [ID]           BIGINT         IDENTITY (1, 1) NOT NULL,
    [IPAddress]    NVARCHAR (MAX) NULL,
    [BrowserType]  NVARCHAR (MAX) NULL,
    [Language]     NVARCHAR (MAX) NULL,
    [IsBot]        BIT            NULL,
    [Country]      NVARCHAR (MAX) NULL,
    [ReferringURL] NVARCHAR (MAX) NULL,
    [SearchString] NVARCHAR (MAX) NULL,
    [Timestamp]    DATETIME       NULL,
    CONSTRAINT [PK_dbo.LogVisitor] PRIMARY KEY CLUSTERED ([ID] ASC)
);

