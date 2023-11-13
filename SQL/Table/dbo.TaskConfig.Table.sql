USE [WPH_DB]
GO
/****** Object:  Table [dbo].[TaskConfig]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TaskConfig](
	[TaskConfigId] [varchar](50) NOT NULL,
	[TaskName] [nvarchar](50) NULL,
	[TaskTrigger] [nvarchar](200) NULL,
	[TaskType] [nvarchar](3) NULL,
	[CompleteCondition] [nvarchar](200) NULL,
	[PopUp] [nvarchar](3) NULL,
	[SecurityLevelCode] [nvarchar](3) NULL,
	[Visible] [bit] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_TaskConfig] PRIMARY KEY CLUSTERED 
(
	[TaskConfigId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
