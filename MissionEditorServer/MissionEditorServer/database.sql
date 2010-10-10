/****** Object:  ForeignKey [FK_Comment_Mission]    Script Date: 03/23/2009 04:35:20 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Comment_Mission]') AND parent_object_id = OBJECT_ID(N'[dbo].[Comment]'))
ALTER TABLE [dbo].[Comment] DROP CONSTRAINT [FK_Comment_Mission]
GO
/****** Object:  ForeignKey [FK_Rating_Mission]    Script Date: 03/23/2009 04:35:20 ******/
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Rating_Mission]') AND parent_object_id = OBJECT_ID(N'[dbo].[Rating]'))
ALTER TABLE [dbo].[Rating] DROP CONSTRAINT [FK_Rating_Mission]
GO
/****** Object:  Table [dbo].[Comment]    Script Date: 03/23/2009 04:35:20 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Comment]') AND type in (N'U'))
DROP TABLE [dbo].[Comment]
GO
/****** Object:  Table [dbo].[Rating]    Script Date: 03/23/2009 04:35:20 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Rating]') AND type in (N'U'))
DROP TABLE [dbo].[Rating]
GO
/****** Object:  Table [dbo].[Mission]    Script Date: 03/23/2009 04:35:20 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Mission]') AND type in (N'U'))
DROP TABLE [dbo].[Mission]
GO
/****** Object:  Table [dbo].[Mission]    Script Date: 03/23/2009 04:35:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Mission]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Mission](
	[Name] [nvarchar](50) COLLATE Czech_CI_AS NOT NULL,
	[Mod] [nvarchar](50) COLLATE Czech_CI_AS NOT NULL,
	[Map] [nvarchar](50) COLLATE Czech_CI_AS NOT NULL,
	[Version] [nvarchar](50) COLLATE Czech_CI_AS NULL,
	[Author] [nvarchar](50) COLLATE Czech_CI_AS NOT NULL,
	[Password] [nvarchar](50) COLLATE Czech_CI_AS NOT NULL,
	[Mutator] [varbinary](max) NOT NULL,
	[Image] [varbinary](max) NULL,
	[Description] [text] COLLATE Czech_CI_AS NULL,
	[Rating] [real] NOT NULL,
	[DownloadCount] [int] NOT NULL,
 CONSTRAINT [PK_Mission] PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[Rating]    Script Date: 03/23/2009 04:35:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Rating]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Rating](
	[Name] [nvarchar](50) COLLATE Czech_CI_AS NOT NULL,
	[IP] [nvarchar](50) COLLATE Czech_CI_AS NOT NULL,
	[Rating] [float] NOT NULL,
 CONSTRAINT [PK_Rating] PRIMARY KEY CLUSTERED 
(
	[Name] ASC,
	[IP] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  Table [dbo].[Comment]    Script Date: 03/23/2009 04:35:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Comment]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Comment](
	[Name] [nvarchar](50) COLLATE Czech_CI_AS NOT NULL,
	[Nick] [nvarchar](50) COLLATE Czech_CI_AS NOT NULL,
	[Text] [text] COLLATE Czech_CI_AS NOT NULL,
	[Time] [datetime] NOT NULL,
	[CommentID] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED 
(
	[CommentID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
END
GO
/****** Object:  ForeignKey [FK_Comment_Mission]    Script Date: 03/23/2009 04:35:20 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Comment_Mission]') AND parent_object_id = OBJECT_ID(N'[dbo].[Comment]'))
ALTER TABLE [dbo].[Comment]  WITH CHECK ADD  CONSTRAINT [FK_Comment_Mission] FOREIGN KEY([Name])
REFERENCES [dbo].[Mission] ([Name])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Comment_Mission]') AND parent_object_id = OBJECT_ID(N'[dbo].[Comment]'))
ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_Comment_Mission]
GO
/****** Object:  ForeignKey [FK_Rating_Mission]    Script Date: 03/23/2009 04:35:20 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Rating_Mission]') AND parent_object_id = OBJECT_ID(N'[dbo].[Rating]'))
ALTER TABLE [dbo].[Rating]  WITH CHECK ADD  CONSTRAINT [FK_Rating_Mission] FOREIGN KEY([Name])
REFERENCES [dbo].[Mission] ([Name])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Rating_Mission]') AND parent_object_id = OBJECT_ID(N'[dbo].[Rating]'))
ALTER TABLE [dbo].[Rating] CHECK CONSTRAINT [FK_Rating_Mission]
GO
