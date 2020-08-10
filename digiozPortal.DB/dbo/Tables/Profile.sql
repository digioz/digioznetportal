CREATE TABLE [dbo].[Profile] (
    [ID]              INT            IDENTITY (1, 1) NOT NULL,
    [UserID]          NVARCHAR (128) NULL,
    [Email]           NVARCHAR (MAX) NULL,
    [Birthday]        DATETIME       NULL,
    [BirthdayVisible] BIT            NULL,
    [Address]         NVARCHAR (MAX) NULL,
    [Address2]        NVARCHAR (MAX) NULL,
    [City]            NVARCHAR (MAX) NULL,
    [State]           NVARCHAR (MAX) NULL,
    [Zip]             NVARCHAR (MAX) NULL,
    [Country]         NVARCHAR (MAX) NULL,
    [Signature]       NVARCHAR (MAX) NULL,
    [Avatar]          NVARCHAR (MAX) NULL,
    [FirstName]       NVARCHAR (MAX) NULL,
    [LastName]        NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.Profile] PRIMARY KEY CLUSTERED ([ID] ASC)
);

