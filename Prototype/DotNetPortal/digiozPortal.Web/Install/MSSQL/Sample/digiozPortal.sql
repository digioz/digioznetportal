USE [digiozPortal]
GO
/****** Object:  Table [dbo].[Announcement]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Announcement](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [nvarchar](128) NULL,
	[Title] [nvarchar](max) NULL,
	[Body] [nvarchar](max) NULL,
	[Visible] [bit] NOT NULL,
	[Timestamp] [datetime] NULL,
 CONSTRAINT [PK_dbo.Announcement] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Chat]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Chat](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [nvarchar](128) NULL,
	[Message] [nvarchar](max) NULL,
	[Timestamp] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.Chat] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Comment]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Comment](
	[Id] [nvarchar](128) NOT NULL,
	[ParentId] [nvarchar](max) NULL,
	[UserId] [nvarchar](max) NULL,
	[Username] [nvarchar](max) NULL,
	[ReferenceId] [nvarchar](max) NULL,
	[ReferenceType] [nvarchar](max) NULL,
	[Body] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[Likes] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Comment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CommentConfig]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommentConfig](
	[Id] [nvarchar](128) NOT NULL,
	[ReferenceId] [nvarchar](max) NULL,
	[ReferenceType] [nvarchar](max) NULL,
	[ReferenceTitle] [nvarchar](max) NULL,
	[Visible] [bit] NOT NULL,
	[Timestamp] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.CommentConfig] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CommentLike]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommentLike](
	[Id] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](max) NULL,
	[CommentId] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.CommentLike] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Config]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Config](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ConfigKey] [nvarchar](max) NOT NULL,
	[ConfigValue] [nvarchar](max) NOT NULL,
	[IsEncrypted] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Config] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Link]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Link](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[URL] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[LinkCategoryID] [int] NOT NULL,
	[Visible] [bit] NULL,
	[Timestamp] [datetime] NULL,
 CONSTRAINT [PK_dbo.Link] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LinkCategory]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LinkCategory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Visible] [bit] NULL,
	[Timestamp] [datetime] NULL,
 CONSTRAINT [PK_dbo.LinkCategory] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Log]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Log](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Message] [nvarchar](max) NULL,
	[Timestamp] [datetime] NULL,
 CONSTRAINT [PK_dbo.Log] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LogVisitor]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LogVisitor](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[IPAddress] [nvarchar](max) NULL,
	[BrowserType] [nvarchar](max) NULL,
	[Language] [nvarchar](max) NULL,
	[IsBot] [bit] NULL,
	[Country] [nvarchar](max) NULL,
	[ReferringURL] [nvarchar](max) NULL,
	[SearchString] [nvarchar](max) NULL,
	[Timestamp] [datetime] NULL,
 CONSTRAINT [PK_dbo.LogVisitor] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MailingList]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MailingList](
	[ID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[DefaultEmailFrom] [nvarchar](50) NOT NULL,
	[DefaultFromName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Address] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_dbo.MailingList] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MailingListCampaign]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MailingListCampaign](
	[ID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Subject] [nvarchar](255) NOT NULL,
	[FromName] [nvarchar](50) NOT NULL,
	[FromEmail] [nvarchar](50) NOT NULL,
	[Summary] [nvarchar](255) NOT NULL,
	[Banner] [nvarchar](255) NULL,
	[Body] [nvarchar](max) NOT NULL,
	[VisitorCount] [int] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.MailingListCampaign] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MailingListCampaignRelation]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MailingListCampaignRelation](
	[ID] [uniqueidentifier] NOT NULL,
	[MailingListID] [uniqueidentifier] NOT NULL,
	[MailingListCampaignID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_dbo.MailingListCampaignRelation] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MailingListSubscriber]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MailingListSubscriber](
	[ID] [uniqueidentifier] NOT NULL,
	[Email] [nvarchar](50) NOT NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[Status] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[DateModified] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.MailingListSubscriber] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MailingListSubscriberRelation]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MailingListSubscriberRelation](
	[ID] [uniqueidentifier] NOT NULL,
	[MailingListID] [uniqueidentifier] NOT NULL,
	[MailingListSubscriberID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_dbo.MailingListSubscriberRelation] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Menu]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Menu](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [nvarchar](128) NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Location] [nvarchar](max) NOT NULL,
	[Controller] [nvarchar](max) NULL,
	[Action] [nvarchar](max) NULL,
	[URL] [nvarchar](max) NULL,
	[Target] [nvarchar](max) NULL,
	[Visible] [bit] NOT NULL,
	[Timestamp] [datetime] NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Menu] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Module]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Module](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [nvarchar](128) NULL,
	[Location] [nvarchar](max) NULL,
	[Title] [nvarchar](max) NULL,
	[Body] [nvarchar](max) NULL,
	[Visible] [bit] NOT NULL,
	[Timestamp] [datetime] NULL,
	[DisplayInBox] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Module] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Order]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Order](
	[ID] [nvarchar](128) NOT NULL,
	[UserID] [nvarchar](128) NOT NULL,
	[InvoiceNumber] [nvarchar](20) NULL,
	[OrderDate] [datetime] NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[ShippingAddress] [nvarchar](70) NOT NULL,
	[ShippingAddress2] [nvarchar](70) NULL,
	[ShippingCity] [nvarchar](40) NOT NULL,
	[ShippingState] [nvarchar](40) NULL,
	[ShippingZip] [nvarchar](30) NOT NULL,
	[ShippingCountry] [nvarchar](50) NOT NULL,
	[BillingAddress] [nvarchar](70) NOT NULL,
	[BillingAddress2] [nvarchar](70) NULL,
	[BillingCity] [nvarchar](40) NOT NULL,
	[BillingState] [nvarchar](40) NOT NULL,
	[BillingZip] [nvarchar](30) NOT NULL,
	[BillingCountry] [nvarchar](50) NOT NULL,
	[Phone] [nvarchar](30) NULL,
	[Email] [nvarchar](255) NOT NULL,
	[Total] [decimal](18, 2) NOT NULL,
	[CCNumber] [nvarchar](100) NULL,
	[CCExp] [nvarchar](10) NULL,
	[CCCardCode] [nvarchar](10) NULL,
	[CCAmount] [decimal](18, 2) NULL,
	[TrxDescription] [nvarchar](max) NULL,
	[TrxApproved] [bit] NOT NULL,
	[TrxAuthorizationCode] [nvarchar](100) NULL,
	[TrxMessage] [nvarchar](max) NULL,
	[TrxResponseCode] [nvarchar](10) NULL,
	[TrxID] [nvarchar](50) NULL,
 CONSTRAINT [PK_dbo.Order] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderDetail]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderDetail](
	[ID] [nvarchar](128) NOT NULL,
	[OrderID] [nvarchar](128) NOT NULL,
	[ProductID] [nvarchar](128) NOT NULL,
	[Quantity] [int] NOT NULL,
	[UnitPrice] [decimal](18, 2) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Size] [nvarchar](50) NULL,
	[Color] [nvarchar](50) NULL,
	[MaterialType] [nvarchar](50) NULL,
	[Notes] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.OrderDetail] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Page]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Page](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [nvarchar](128) NULL,
	[Title] [nvarchar](max) NULL,
	[URL] [nvarchar](max) NULL,
	[Body] [nvarchar](max) NULL,
	[Keywords] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Visible] [bit] NOT NULL,
	[Timestamp] [datetime] NULL,
 CONSTRAINT [PK_dbo.Page] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Picture]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Picture](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[UserID] [nvarchar](128) NULL,
	[AlbumID] [bigint] NULL,
	[Filename] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Timestamp] [datetime] NULL,
	[Approved] [bit] NULL,
	[Visible] [bit] NULL,
 CONSTRAINT [PK_dbo.Picture] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PictureAlbum]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PictureAlbum](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Timestamp] [datetime] NULL,
	[Visible] [bit] NULL,
 CONSTRAINT [PK_dbo.PictureAlbum] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Plugin]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Plugin](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[DLL] [nvarchar](max) NULL,
	[IsEnabled] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Plugin] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Poll]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Poll](
	[Id] [uniqueidentifier] NOT NULL,
	[Slug] [nvarchar](max) NULL,
	[IsClosed] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[Featured] [bit] NOT NULL,
	[AllowMultipleOptionsVote] [bit] NOT NULL,
	[MembershipUser_Id] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.Poll] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PollAnswer]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PollAnswer](
	[Id] [uniqueidentifier] NOT NULL,
	[Answer] [nvarchar](max) NULL,
	[Poll_Id] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_dbo.PollAnswer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PollUsersVote]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PollUsersVote](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](max) NULL,
	[PollId] [nvarchar](max) NULL,
	[DateVoted] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.PollUsersVote] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PollVote]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PollVote](
	[Id] [uniqueidentifier] NOT NULL,
	[PollAnswer_Id] [uniqueidentifier] NOT NULL,
	[MembershipUser_Id] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.PollVote] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Product]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Product](
	[ID] [nvarchar](128) NOT NULL,
	[ProductCategoryID] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Make] [nvarchar](50) NULL,
	[Model] [nvarchar](50) NULL,
	[SKU] [nvarchar](50) NULL,
	[Image] [nvarchar](50) NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Cost] [decimal](18, 2) NULL,
	[QuantityPerUnit] [int] NULL,
	[Weight] [nvarchar](20) NULL,
	[Dimensions] [nvarchar](50) NULL,
	[Sizes] [nvarchar](50) NULL,
	[Colors] [nvarchar](50) NULL,
	[MaterialType] [nvarchar](50) NULL,
	[PartNumber] [nvarchar](50) NULL,
	[ShortDescription] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[ManufacturerURL] [nvarchar](max) NULL,
	[UnitsInStock] [int] NOT NULL,
	[OutOfStock] [bit] NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[Visible] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Product] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductCategory]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductCategory](
	[ID] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Visible] [bit] NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.ProductCategory] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductOption]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductOption](
	[ID] [nvarchar](128) NOT NULL,
	[ProductID] [nvarchar](128) NOT NULL,
	[OptionType] [nvarchar](50) NOT NULL,
	[OptionValue] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_dbo.ProductOption] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Profile]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Profile](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [nvarchar](128) NULL,
	[Email] [nvarchar](max) NULL,
	[Birthday] [datetime] NULL,
	[BirthdayVisible] [bit] NULL,
	[Address] [nvarchar](max) NULL,
	[Address2] [nvarchar](max) NULL,
	[City] [nvarchar](max) NULL,
	[State] [nvarchar](max) NULL,
	[Zip] [nvarchar](max) NULL,
	[Country] [nvarchar](max) NULL,
	[Signature] [nvarchar](max) NULL,
	[Avatar] [nvarchar](max) NULL,
	[FirstName] [nvarchar](max) NULL,
	[LastName] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Profile] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RSS]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RSS](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Url] [nvarchar](max) NOT NULL,
	[MaxCount] [int] NOT NULL,
	[Timestamp] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.RSS] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShoppingCart]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShoppingCart](
	[ID] [nvarchar](128) NOT NULL,
	[UserID] [nvarchar](128) NOT NULL,
	[ProductID] [nvarchar](128) NOT NULL,
	[Quantity] [int] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[Size] [nvarchar](50) NULL,
	[Color] [nvarchar](50) NULL,
	[MaterialType] [nvarchar](50) NULL,
	[Notes] [nvarchar](max) NULL,
	[AspNetUser_Id] [nvarchar](128) NULL,
 CONSTRAINT [PK_dbo.ShoppingCart] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SlideShow]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SlideShow](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Image] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.SlideShow] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Video]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Video](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[UserID] [nvarchar](128) NULL,
	[AlbumID] [bigint] NULL,
	[Filename] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Timestamp] [datetime] NULL,
	[Approved] [bit] NULL,
	[Visible] [bit] NULL,
	[Thumbnail] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Video] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VideoAlbum]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VideoAlbum](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Timestamp] [datetime] NULL,
	[Visible] [bit] NULL,
 CONSTRAINT [PK_dbo.VideoAlbum] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VisitorInfo]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VisitorInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IPAddress] [nvarchar](25) NOT NULL,
	[PageURL] [nvarchar](max) NULL,
	[ReferringURL] [nvarchar](max) NULL,
	[BrowserName] [nvarchar](100) NULL,
	[BrowserType] [nvarchar](100) NULL,
	[BrowserUserAgent] [nvarchar](max) NULL,
	[BrowserVersion] [nvarchar](20) NULL,
	[IsCrawler] [bit] NOT NULL,
	[JSVersion] [nvarchar](max) NULL,
	[OperatingSystem] [nvarchar](20) NULL,
	[Keywords] [nvarchar](max) NULL,
	[SearchEngine] [nvarchar](20) NULL,
	[Country] [nvarchar](30) NULL,
	[Language] [nvarchar](100) NULL,
	[Timestamp] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.VisitorInfo] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VisitorSession]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VisitorSession](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IpAddress] [nvarchar](25) NOT NULL,
	[PageUrl] [nvarchar](max) NULL,
	[SessionId] [nvarchar](max) NULL,
	[UserName] [nvarchar](max) NULL,
	[DateCreated] [datetime] NOT NULL,
	[DateModified] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.VisitorSession] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Zone]    Script Date: 8/10/2020 2:27:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Zone](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[ZoneType] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_dbo.Zone] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
INSERT [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'd9c4a3bb-6cea-4d34-80e9-d9ba81905f34', N'webmaster@digioz.com', N'WEBMASTER@DIGIOZ.COM', N'webmaster@digioz.com', N'WEBMASTER@DIGIOZ.COM', 0, N'AQAAAAEAACcQAAAAEDhvep1O2YQBcOhCiLujXpRLfv/jWDVY1PxjerKJph8qF87thLK83+UpvE8ppj61Ig==', N'F3UWCAGEVTZC3O6S4WGRNKMD5RFV6N7R', N'40128aa3-57f3-4232-9643-251123fcb545', NULL, 0, 0, NULL, 1, 0)
GO
SET IDENTITY_INSERT [dbo].[Config] ON 

INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (1, N'SMTPServer', N'mail.domain.com', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (2, N'SMTPPort', N'587', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (3, N'SMTPUsername', N'webmaster@domain.com', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (4, N'SMTPPassword', N'ohuiChQV5zxEwpmisiiWcIvlEVBH/zaroX1Rd9SD6zU=', 1)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (5, N'SiteURL', N'http://localhost:3588', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (6, N'SiteName', N'DigiOz .NET Portal', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (7, N'SiteEncryptionKey', N'BlAMwXxp7oMxGtWzIZYe', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (8, N'WebmasterEmail', N'webmaster@domain.com', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (9, N'PaymentLoginID', N'6b74ZBkn5u3', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (10, N'PaymentTransactionKey', N'9M4Tc3s89w3C39cq', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (11, N'PaymentTestMode', N'true', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (12, N'TwitterHandle', N'@digioz', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (13, N'PaymentTransactionFee', N'0', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (14, N'NumberOfAnnouncements', N'2', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (15, N'ShowContactForm', N'false', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (16, N'VisitorSessionPurgePeriod', N'30', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (17, N'PaypalMode', N'sandbox', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (18, N'PaypalClientId', N'AfBQZ3rwN5BKZN6QOJL4zBa1-Uph0KpxxrpMz2ro9RQO_W_CT_1-31GaM-iNo5S0WxIO4Z-LJtW5RInf', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (19, N'PaypalClientSecret', N'EGpl6DrqoaOWVysXEatofIjglg1i1XwHwSIhcw7jZ8duvfgxZAI6SeE8TVmbgHOXxJB7pyKW2O5cOhqj', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (20, N'PaypalConnectionTimeout', N'360000', 0)
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (21, N'EnableCommentsOnAllPages', N'true', 0)
SET IDENTITY_INSERT [dbo].[Config] OFF
GO
SET IDENTITY_INSERT [dbo].[Menu] ON 

INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (1, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Home', N'TopMenu', N'Home', N'Index', NULL, NULL, 1, CAST(N'2018-04-30T20:20:24.673' AS DateTime), 1)
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (2, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'About', N'TopMenu', N'Home', N'About', NULL, NULL, 1, CAST(N'2018-04-30T20:20:24.720' AS DateTime), 2)
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (3, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Contact', N'TopMenu', N'Home', N'Contact', NULL, NULL, 0, CAST(N'2018-04-30T20:20:24.720' AS DateTime), 3)
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (4, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Forum', N'TopMenu', N'Forum', N'Index', NULL, NULL, 1, CAST(N'2018-04-30T20:20:24.720' AS DateTime), 4)
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (5, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Links', N'TopMenu', N'Links', N'Index', NULL, NULL, 1, CAST(N'2018-04-30T20:20:24.720' AS DateTime), 5)
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (6, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Chat', N'TopMenu', N'Chat', N'Index', NULL, NULL, 1, CAST(N'2018-04-30T20:20:24.720' AS DateTime), 6)
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (7, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Store', N'TopMenu', N'Store', N'Index', NULL, NULL, 0, CAST(N'2018-04-30T20:20:24.720' AS DateTime), 7)
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (8, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Twitter', N'TopMenu', N'Twitter', N'Index', NULL, NULL, 0, CAST(N'2018-04-30T20:20:24.720' AS DateTime), 8)
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (9, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Home', N'LeftMenu', N'Home', N'Index', NULL, NULL, 1, CAST(N'2018-04-30T20:20:24.720' AS DateTime), 9)
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (10, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Pictures', N'LeftMenu', N'Pictures', N'Index', NULL, NULL, 1, CAST(N'2018-04-30T20:20:24.720' AS DateTime), 10)
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (11, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Videos', N'LeftMenu', N'Videos', N'Index', NULL, NULL, 1, CAST(N'2018-04-30T20:20:24.720' AS DateTime), 11)
SET IDENTITY_INSERT [dbo].[Menu] OFF
GO
SET IDENTITY_INSERT [dbo].[Page] ON 

INSERT [dbo].[Page] ([ID], [UserID], [Title], [URL], [Body], [Keywords], [Description], [Visible], [Timestamp]) VALUES (1, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Home', N'/Home/Index', N'<p><span style=''font-size: medium;''><strong>Welcome to DigiOz .NET Portal!</strong></span></p>
<p>DigiOz .NET Portal is a web based portal system written in ASP.NET MVC 5 and&nbsp;C#&nbsp;which uses a Microsoft SQL Database to allows webmasters to setup and customize an instant website for either business or personal use.</p>
<p>Some Features included in this Portal System include:</p>
<ul>
<li>An Administrative Dashboard, where the Webmaster can Manage the Site and related Features.</li>
<li>A Page Manager, to allow Admins to Create new Pages, Edit existing Pages or Delete Them.</li>
<li>A Database Driven Configuration System to fine tune the Portal System</li>
<li>Some Database Utilities to help Manage the Site Database</li>
<li>File Manager, which allows you to add or remove files to your site.</li>
<li>Module Manager, allow you to install new Plugins to the Portal.</li>
<li>Forum Manager allows you to Manage Forum Posts, Threads, and Users.</li>
<li>Poll Manager lets you create new polls to display on the site.</li>
<li>The Statistics section lets you see site related statistics such as the number of visitors, number of page Views, etc.</li>
<li>Menu Manager lets you add new Menu links both to internal pages and external sites.</li>
<li>User Manager lets you manage the registered users of the site.</li>
<li>Announcements section let''''s you add, edit or remove site wide announcements to the users, which show up on the Home Index Page.</li>
<li>Picture Manager lets you create Picture Galleries, and add or remove pictures from the site.</li>
<li>Video Manager allows you to upload and display Videos to your site and manage them.</li>
<li>Link Manager allows you to create a links page to do link exchagne with other sites similar to yours.</li>
<li>Chat Manager lets you manage the Chat Database Table.</li>
</ul>
<p><strong><span style=''font-size: medium;''>About DigiOz Multimedia, Inc</span></strong></p>
<p><strong><span style=''font-size: medium;''></span></strong>DigiOz Multimedia, Inc is a Chicago, Illinois USA based Software Development Company which provides web design for personal and business use, CRM, custom programming for web and PC, design database driven systems for clients, as well as business process modeling and consulting. We also have an active Open Source Community that provides many IT Systems and Web Portals as Open Source Products for Everyone to share and enjoy.</p>
<p>Visit us at <a href=''http://www.digioz.com''>www.digioz.com</a> for more information, or email us at <a href=''mailto:support@digioz.com''>support@digioz.com</a>. </p>', N'DigiOz .NET Portal, CMS, Portal, Web Portal, ASP.NET MVC', N'DigiOz .NET Portal is a web based portal system written in ASP.NET MVC 5 and C# which uses a Microsoft SQL Database to allows webmasters to setup and customize an instant website for either business or personal use.', 1, CAST(N'2018-04-30T20:20:24.720' AS DateTime))
INSERT [dbo].[Page] ([ID], [UserID], [Title], [URL], [Body], [Keywords], [Description], [Visible], [Timestamp]) VALUES (2, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Contact', N'/Home/Contact', N'<h2>Contact</h2>
<h3>Below is our Contact Information:</h3>
<address>One Microsoft Way<br /> Redmond, WA 98052-6399<br /> <abbr title=''Phone''>P:</abbr> 425.555.0100</address><address><strong>Support:</strong> <a href=''mailto:Support@example.com''>Support@example.com</a><br /> <strong>Marketing:</strong> <a href=''mailto:Marketing@example.com''>Marketing@example.com</a></address>', NULL, NULL, 1, CAST(N'2018-04-30T20:20:24.767' AS DateTime))
INSERT [dbo].[Page] ([ID], [UserID], [Title], [URL], [Body], [Keywords], [Description], [Visible], [Timestamp]) VALUES (3, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'About', N'/Home/About', N'<h2>About</h2>
<h3>Some information about us:</h3>
<p>Use this area to provide additional information.</p>', NULL, NULL, 1, CAST(N'2018-04-30T20:20:24.767' AS DateTime))
SET IDENTITY_INSERT [dbo].[Page] OFF
GO
SET IDENTITY_INSERT [dbo].[Plugin] ON 

INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (1, N'Chat', NULL, 1)
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (2, N'Store', NULL, 1)
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (3, N'Twitter', NULL, 0)
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (4, N'WhoIsOnline', NULL, 1)
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (5, N'SlideShow', NULL, 0)
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (6, N'Comments', NULL, 0)
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (7, N'RSSFeed', NULL, 0)
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (1002, N'LatestPictures', NULL, 1)
SET IDENTITY_INSERT [dbo].[Plugin] OFF
GO
SET IDENTITY_INSERT [dbo].[Zone] ON 

INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (1, N'Top', N'Module')
INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (2, N'TopMenu', N'Menu')
INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (3, N'Left', N'Module')
INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (4, N'LeftMenu', N'Menu')
INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (5, N'BodyTop', N'Module')
INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (6, N'BodyBottom', N'Module')
INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (7, N'Bottom', N'Module')
SET IDENTITY_INSERT [dbo].[Zone] OFF
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
