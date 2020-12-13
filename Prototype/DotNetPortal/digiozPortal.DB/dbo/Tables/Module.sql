CREATE TABLE [dbo].[Module] (
    [ID]           INT            IDENTITY (1, 1) NOT NULL,
    [UserID]       NVARCHAR (128) NULL,
    [Location]     NVARCHAR (MAX) NULL,
    [Title]        NVARCHAR (MAX) NULL,
    [Body]         NVARCHAR (MAX) NULL,
    [Visible]      BIT            NOT NULL,
    [Timestamp]    DATETIME       NULL,
    [DisplayInBox] BIT            NOT NULL,
    CONSTRAINT [PK_dbo.Module] PRIMARY KEY CLUSTERED ([ID] ASC)
);

