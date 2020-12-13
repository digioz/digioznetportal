CREATE TABLE [dbo].[Menu] (
    [ID]         INT            IDENTITY (1, 1) NOT NULL,
    [UserID]     NVARCHAR (128) NULL,
    [Name]       NVARCHAR (MAX) NOT NULL,
    [Location]   NVARCHAR (MAX) NOT NULL,
    [Controller] NVARCHAR (MAX) NULL,
    [Action]     NVARCHAR (MAX) NULL,
    [URL]        NVARCHAR (MAX) NULL,
    [Target]     NVARCHAR (MAX) NULL,
    [Visible]    BIT            NOT NULL,
    [Timestamp]  DATETIME       NULL,
    [SortOrder]  INT            NOT NULL,
    CONSTRAINT [PK_dbo.Menu] PRIMARY KEY CLUSTERED ([ID] ASC)
);

