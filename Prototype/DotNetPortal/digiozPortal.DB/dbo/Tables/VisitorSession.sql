CREATE TABLE [dbo].[VisitorSession] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [IpAddress]    NVARCHAR (25)  NOT NULL,
    [PageUrl]      NVARCHAR (MAX) NULL,
    [SessionId]    NVARCHAR (MAX) NULL,
    [UserName]     NVARCHAR (MAX) NULL,
    [DateCreated]  DATETIME       NOT NULL,
    [DateModified] DATETIME       NOT NULL,
    CONSTRAINT [PK_dbo.VisitorSession] PRIMARY KEY CLUSTERED ([Id] ASC)
);

