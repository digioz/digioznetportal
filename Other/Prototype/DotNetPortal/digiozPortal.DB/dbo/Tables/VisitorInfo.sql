CREATE TABLE [dbo].[VisitorInfo] (
    [ID]               INT            IDENTITY (1, 1) NOT NULL,
    [IPAddress]        NVARCHAR (25)  NOT NULL,
    [PageURL]          NVARCHAR (MAX) NULL,
    [ReferringURL]     NVARCHAR (MAX) NULL,
    [BrowserName]      NVARCHAR (100) NULL,
    [BrowserType]      NVARCHAR (100) NULL,
    [BrowserUserAgent] NVARCHAR (MAX) NULL,
    [BrowserVersion]   NVARCHAR (20)  NULL,
    [IsCrawler]        BIT            NOT NULL,
    [JSVersion]        NVARCHAR (MAX) NULL,
    [OperatingSystem]  NVARCHAR (20)  NULL,
    [Keywords]         NVARCHAR (MAX) NULL,
    [SearchEngine]     NVARCHAR (20)  NULL,
    [Country]          NVARCHAR (30)  NULL,
    [Language]         NVARCHAR (100) NULL,
    [Timestamp]        DATETIME       NOT NULL,
    CONSTRAINT [PK_dbo.VisitorInfo] PRIMARY KEY CLUSTERED ([ID] ASC)
);

