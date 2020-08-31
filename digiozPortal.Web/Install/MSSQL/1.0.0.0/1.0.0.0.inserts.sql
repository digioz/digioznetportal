INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'2705f8c1-3194-4055-b55d-9737570391fd', N'Administrator', N'Administrator', NULL)
GO
INSERT [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'5b479ec1-6934-4dfa-9bae-f7dffade0836', N'user1@mail.com', N'USER1@MAIL.COM', N'user1@mail.com', N'USER1@MAIL.COM', 0, N'AQAAAAEAACcQAAAAEJISeglNmBnfpwu6BJXd7jSh9jxeNdX2CXKzSZGUoOIJvRBG4nH5O2NnsGHFbMYcIA==', N'3RCCOF64EJGX4UZTBISAL4PKITHDTWQG', N'4d3acfa8-989c-4daa-af5e-444d8997edf9', NULL, 0, 0, NULL, 1, 0)
GO
INSERT [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'd9c4a3bb-6cea-4d34-80e9-d9ba81905f34', N'admin@domain.com', N'ADMIN@DOMAIN.COM', N'admin@domain.com', N'ADMIN@DOMAIN.COM', 0, N'AQAAAAEAACcQAAAAEJISeglNmBnfpwu6BJXd7jSh9jxeNdX2CXKzSZGUoOIJvRBG4nH5O2NnsGHFbMYcIA==', N'3RCCOF64EJGX4UZTBISAL4PKITHDTWQG', N'4d3acfa8-989c-4daa-af5e-444d8997edf9', NULL, 0, 0, NULL, 1, 0)
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'd9c4a3bb-6cea-4d34-80e9-d9ba81905f34', N'2705f8c1-3194-4055-b55d-9737570391fd')
GO
SET IDENTITY_INSERT [dbo].[Announcement] ON 

GO
INSERT [dbo].[Announcement] ([ID], [UserID], [Title], [Body], [Visible], [Timestamp]) VALUES (1, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Test Announcement 1', N'<p>This is a test Announcement.</p>', 1, CAST(N'2018-04-30 20:20:24.643' AS DateTime))
GO
INSERT [dbo].[Announcement] ([ID], [UserID], [Title], [Body], [Visible], [Timestamp]) VALUES (2, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Test Announcement 2', N'<p>Test Announcement 2 Body</p>', 1, CAST(N'2018-04-30 20:20:24.673' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Announcement] OFF
GO
INSERT [dbo].[Comment] ([Id], [ParentId], [UserId], [Username], [ReferenceId], [ReferenceType], [Body], [CreatedDate], [ModifiedDate], [Likes]) VALUES (N'5e695c1e-54de-48f5-b25c-03d46883b401', NULL, N'd9c4a3bb-6cea-4d34-80e9-d9ba81905f34', N'admin@domain.com', N'2', N'Announcement', N'Test reply 1', CAST(N'2020-08-19 22:16:40.753' AS DateTime), CAST(N'2020-08-19 22:16:40.753' AS DateTime), 0)
GO
INSERT [dbo].[Comment] ([Id], [ParentId], [UserId], [Username], [ReferenceId], [ReferenceType], [Body], [CreatedDate], [ModifiedDate], [Likes]) VALUES (N'c3191791-73e4-4fad-9d1f-2b524ae3d783', NULL, N'd9c4a3bb-6cea-4d34-80e9-d9ba81905f34', N'admin@domain.com', N'2', N'Announcement', N'Test reply 2. ', CAST(N'2020-08-20 21:30:05.740' AS DateTime), CAST(N'2020-08-20 21:30:05.740' AS DateTime), 4)
GO
INSERT [dbo].[CommentLike] ([Id], [UserId], [CommentId]) VALUES (N'2dc237cf-3d7d-4a05-b4ba-d708cb3cd508', N'd9c4a3bb-6cea-4d34-80e9-d9ba81905f34', N'c3191791-73e4-4fad-9d1f-2b524ae3d783')
GO
INSERT [dbo].[CommentLike] ([Id], [UserId], [CommentId]) VALUES (N'2e5acdbe-38b3-4fe1-9e1b-cf21f6dfb5eb', N'd9c4a3bb-6cea-4d34-80e9-d9ba81905f34', N'c3191791-73e4-4fad-9d1f-2b524ae3d783')
GO
INSERT [dbo].[CommentLike] ([Id], [UserId], [CommentId]) VALUES (N'4079d862-2845-4039-9cf3-b18ce9bc1146', N'd9c4a3bb-6cea-4d34-80e9-d9ba81905f34', N'c3191791-73e4-4fad-9d1f-2b524ae3d783')
GO
INSERT [dbo].[CommentLike] ([Id], [UserId], [CommentId]) VALUES (N'79217d0b-70d6-4714-9188-1ee6bd247c23', N'd9c4a3bb-6cea-4d34-80e9-d9ba81905f34', N'c3191791-73e4-4fad-9d1f-2b524ae3d783')
GO
SET IDENTITY_INSERT [dbo].[Config] ON 

GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (1, N'SMTPServer', N'mail.domain.com', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (2, N'SMTPPort', N'587', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (3, N'SMTPUsername', N'webmaster@domain.com', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (4, N'SMTPPassword', N'', 1)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (5, N'SiteURL', N'http://localhost:6969', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (6, N'SiteName', N'DigiOz .NET Portal', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (7, N'SiteEncryptionKey', N'BlAMwXxp7oMxGtWzUEpe', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (8, N'WebmasterEmail', N'webmaster@domain.com', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (9, N'PaymentLoginID', N'', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (10, N'PaymentTransactionKey', N'', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (11, N'PaymentTestMode', N'true', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (12, N'TwitterHandle', N'@digioz', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (13, N'PaymentTransactionFee', N'0', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (14, N'NumberOfAnnouncements', N'2', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (15, N'ShowContactForm', N'false', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (16, N'VisitorSessionPurgePeriod', N'30', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (17, N'PaypalMode', N'sandbox', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (18, N'PaypalClientId', N'', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (19, N'PaypalClientSecret', N'', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (20, N'PaypalConnectionTimeout', N'360000', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (21, N'EnableCommentsOnAllPages', N'true', 0)
GO
INSERT [dbo].[Config] ([ID], [ConfigKey], [ConfigValue], [IsEncrypted]) VALUES (23, N'TinyMCEApiKey', N'no-api-key', 0)
GO
SET IDENTITY_INSERT [dbo].[Config] OFF
GO
SET IDENTITY_INSERT [dbo].[Link] ON 

GO
INSERT [dbo].[Link] ([ID], [Name], [URL], [Description], [LinkCategoryID], [Visible], [Timestamp]) VALUES (5, N'DigiOz Multimedia, Inc', N'https://www.digioz.com', N'Do you have a Software Development Project for your Business or Personal use? Why not give the DigiOz Development Team a try! Our Senior Programmers and Project Managers are ready to take on any Desktop, Web or Mobile Software Development Project that you may have in mind. Contact Us now at support@digioz.com for a price estimate.', 1, 1, CAST(N'2020-08-31 00:00:00.000' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Link] OFF
GO
SET IDENTITY_INSERT [dbo].[LinkCategory] ON 

GO
INSERT [dbo].[LinkCategory] ([ID], [Name], [Visible], [Timestamp]) VALUES (1, N'Main', 1, CAST(N'2020-08-25 08:29:42.137' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[LinkCategory] OFF
GO
INSERT [dbo].[MailingList] ([ID], [Name], [DefaultEmailFrom], [DefaultFromName], [Description], [Address]) VALUES (N'99a0952d-40ea-4ddb-a692-08b9fa7e92f5', N'Mailing List 1', N'admin@domain.com', N'DigiOz Webmaster', N'Our first Mailing list', N'1 Main Street Ste 100
Chicago, IL 60102
USA')
GO
INSERT [dbo].[MailingListCampaign] ([ID], [Name], [Subject], [FromName], [FromEmail], [Summary], [Banner], [Body], [VisitorCount], [DateCreated]) VALUES (N'6680f9a8-a809-49bd-8e9e-a460eb83a7f7', N'Campaign 1', N'Very important News', N'James Bond', N'me@mail.com', N'This is a very important campaign', N'blank_email_banner.png', N'<p><span>Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.</span></p>', 1, CAST(N'2020-08-20 16:35:05.720' AS DateTime))
GO
INSERT [dbo].[MailingListSubscriber] ([ID], [Email], [FirstName], [LastName], [Status], [DateCreated], [DateModified]) VALUES (N'9b04de85-fcc8-44cb-9c70-b5510d048de4', N'digioz@gmail.com', N'Pete', N'Soheil', 1, CAST(N'2020-08-20 16:33:44.507' AS DateTime), CAST(N'2020-08-20 16:33:44.507' AS DateTime))
GO
INSERT [dbo].[MailingListSubscriberRelation] ([ID], [MailingListID], [MailingListSubscriberID]) VALUES (N'cb6d52fb-b7cc-4038-872e-70ef713289af', N'99a0952d-40ea-4ddb-a692-08b9fa7e92f5', N'9b04de85-fcc8-44cb-9c70-b5510d048de4')
GO
SET IDENTITY_INSERT [dbo].[Menu] ON 

GO
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (1, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Home', N'TopMenu', N'Home', N'Index', NULL, NULL, 1, CAST(N'2018-04-30 20:20:24.673' AS DateTime), 1)
GO
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (2, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'About', N'TopMenu', N'Home', N'About', NULL, NULL, 1, CAST(N'2018-04-30 20:20:24.720' AS DateTime), 2)
GO
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (3, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Contact', N'TopMenu', N'Home', N'Contact', NULL, NULL, 1, CAST(N'2018-04-30 20:20:24.720' AS DateTime), 3)
GO
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (4, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Forum', N'TopMenu', N'Forum', N'Index', NULL, NULL, 1, CAST(N'2018-04-30 20:20:24.720' AS DateTime), 4)
GO
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (5, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Links', N'TopMenu', N'Links', N'Index', NULL, NULL, 1, CAST(N'2018-04-30 20:20:24.720' AS DateTime), 5)
GO
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (6, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Chat', N'TopMenu', N'Chat', N'Index', NULL, NULL, 1, CAST(N'2018-04-30 20:20:24.720' AS DateTime), 6)
GO
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (7, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Store', N'TopMenu', N'Store', N'Index', NULL, NULL, 0, CAST(N'2018-04-30 20:20:24.720' AS DateTime), 7)
GO
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (8, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Twitter', N'TopMenu', N'Twitter', N'Index', NULL, NULL, 0, CAST(N'2018-04-30 20:20:24.720' AS DateTime), 8)
GO
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (9, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Home', N'LeftMenu', N'Home', N'Index', NULL, NULL, 1, CAST(N'2018-04-30 20:20:24.720' AS DateTime), 9)
GO
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (10, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Pictures', N'LeftMenu', N'Pictures', N'Index', NULL, NULL, 1, CAST(N'2018-04-30 20:20:24.720' AS DateTime), 10)
GO
INSERT [dbo].[Menu] ([ID], [UserID], [Name], [Location], [Controller], [Action], [URL], [Target], [Visible], [Timestamp], [SortOrder]) VALUES (11, N'd1c447c1-b022-4c92-93e8-ab9aaea65224', N'Videos', N'LeftMenu', N'Videos', N'Index', NULL, NULL, 1, CAST(N'2018-04-30 20:20:24.720' AS DateTime), 11)
GO
SET IDENTITY_INSERT [dbo].[Menu] OFF
GO
SET IDENTITY_INSERT [dbo].[Page] ON 

GO
INSERT [dbo].[Page] ([ID], [UserID], [Title], [URL], [Body], [Keywords], [Description], [Visible], [Timestamp]) VALUES (1, N'd9c4a3bb-6cea-4d34-80e9-d9ba81905f34', N'Home', N'Home', N'<p><span style="font-size: medium;"><strong>Welcome to DigiOz .NET Portal!</strong></span></p><p>DigiOz .NET Portal is a web based portal system written in ASP.NET MVC 5 and&nbsp;C#&nbsp;which uses a Microsoft SQL Database to allows webmasters to setup and customize an instant website for either business or personal use.</p><p>Some Features included in this Portal System include:</p><ul><li>An Administrative Dashboard, where the Webmaster can Manage the Site and related Features.</li><li>A Page Manager, to allow Admins to Create new Pages, Edit existing Pages or Delete Them.</li><li>A Database Driven Configuration System to fine tune the Portal System</li><li>Some Database Utilities to help Manage the Site Database</li><li>File Manager, which allows you to add or remove files to your site.</li><li>Module Manager, allow you to install new Plugins to the Portal.</li><li>Forum Manager allows you to Manage Forum Posts, Threads, and Users.</li><li>Poll Manager lets you create new polls to display on the site.</li><li>The Statistics section lets you see site related statistics such as the number of visitors, number of page Views, etc.</li><li>Menu Manager lets you add new Menu links both to internal pages and external sites.</li><li>User Manager lets you manage the registered users of the site.</li><li>Announcements section let''''s you add, edit or remove site wide announcements to the users, which show up on the Home Index Page.</li><li>Picture Manager lets you create Picture Galleries, and add or remove pictures from the site.</li><li>Video Manager allows you to upload and display Videos to your site and manage them.</li><li>Link Manager allows you to create a links page to do link exchagne with other sites similar to yours.</li><li>Chat Manager lets you manage the Chat Database Table.</li></ul><p><strong><span style="font-size: medium;">About DigiOz Multimedia, Inc</span></strong></p><p>DigiOz Multimedia, Inc is a Chicago, Illinois USA based Software Development Company which provides web design for personal and business use, CRM, custom programming for web and PC, design database driven systems for clients, as well as business process modeling and consulting. We also have an active Open Source Community that provides many IT Systems and Web Portals as Open Source Products for Everyone to share and <strong>enjoy</strong>.</p><p>Visit us at <a href="http://www.digioz.com">www.digioz.com</a> for more information, or email us at <a href="mailto:support@digioz.com">support@digioz.com</a>.</p>', N'DigiOz .NET Portal, CMS, Portal, Web Portal, ASP.NET MVC', N'DigiOz .NET Portal is a web based portal system written in ASP.NET MVC 5 and C# which uses a Microsoft SQL Database to allows webmasters to setup and customize an instant website for either business or personal use.', 1, CAST(N'2020-08-29 20:33:59.287' AS DateTime))
GO
INSERT [dbo].[Page] ([ID], [UserID], [Title], [URL], [Body], [Keywords], [Description], [Visible], [Timestamp]) VALUES (2, N'd9c4a3bb-6cea-4d34-80e9-d9ba81905f34', N'Contact', N'Contact', N'<h2>Contact</h2>
<h3>Below is our Contact Information:</h3>
<address>One Microsoft Way<br /> Redmond, WA 98052-6399<br /> <abbr title=''Phone''>P:</abbr> 425.555.0100</address><address><strong>Support:</strong> <a href=''mailto:Support@example.com''>Support@example.com</a><br /> <strong>Marketing:</strong> <a href=''mailto:Marketing@example.com''>Marketing@example.com</a></address>', NULL, NULL, 1, CAST(N'2018-04-30 20:20:24.767' AS DateTime))
GO
INSERT [dbo].[Page] ([ID], [UserID], [Title], [URL], [Body], [Keywords], [Description], [Visible], [Timestamp]) VALUES (3, N'd9c4a3bb-6cea-4d34-80e9-d9ba81905f34', N'About', N'About', N'<h2>About</h2>
<h3>Some information about us:</h3>
<p>Use this area to provide additional information.</p>', NULL, NULL, 1, CAST(N'2018-04-30 20:20:24.767' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Page] OFF
GO
SET IDENTITY_INSERT [dbo].[Plugin] ON 

GO
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (1, N'Chat', NULL, 1)
GO
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (2, N'Store', NULL, 1)
GO
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (3, N'Twitter', NULL, 0)
GO
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (4, N'WhoIsOnline', NULL, 1)
GO
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (5, N'SlideShow', NULL, 0)
GO
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (6, N'Comments', NULL, 0)
GO
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (7, N'RSSFeed', NULL, 0)
GO
INSERT [dbo].[Plugin] ([Id], [Name], [DLL], [IsEnabled]) VALUES (1002, N'LatestPictures', NULL, 1)
GO
SET IDENTITY_INSERT [dbo].[Plugin] OFF
GO
SET IDENTITY_INSERT [dbo].[Zone] ON 

GO
INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (1, N'Top', N'Module')
GO
INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (2, N'TopMenu', N'Menu')
GO
INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (3, N'Left', N'Module')
GO
INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (4, N'LeftMenu', N'Menu')
GO
INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (5, N'BodyTop', N'Module')
GO
INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (6, N'BodyBottom', N'Module')
GO
INSERT [dbo].[Zone] ([ID], [Name], [ZoneType]) VALUES (7, N'Bottom', N'Module')
GO
SET IDENTITY_INSERT [dbo].[Zone] OFF
GO
