USE [DatabusTest]
GO

/****** Object:  Table [dbo].[Patients]    Script Date: 9/29/2018 8:24:54 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Patients](
	[PatientID] [varchar](50) NOT NULL,
	[PatientLastNM] [varchar](255) NOT NULL,
	[PatientFirstNM] [varchar](255) NOT NULL,
	[DateOfBirth] [datetime] NOT NULL,
 CONSTRAINT [PK_Patients] PRIMARY KEY CLUSTERED 
(
	[PatientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


USE [DatabusTest]
GO

/****** Object:  Table [dbo].[PatientDiagnoses]    Script Date: 9/29/2018 8:25:04 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PatientDiagnoses](
	[DiagnosisID] [int] NOT NULL,
	[PatientID] [varchar](50) NOT NULL,
	[DiagnosisCode] [varchar](50) NOT NULL,
	[DiagnosisDescription] [varchar](255) NOT NULL,
 CONSTRAINT [PK_PatientDiagnoses] PRIMARY KEY CLUSTERED 
(
	[DiagnosisID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

USE [DatabusTest]
GO

/****** Object:  Table [dbo].[PatientLabs]    Script Date: 9/29/2018 8:25:17 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PatientLabs](
	[LabID] [int] NOT NULL,
	[PatientID] [varchar](50) NOT NULL,
	[LabCode] [varchar](50) NOT NULL,
	[LabResult] [float] NOT NULL,
 CONSTRAINT [PK_PatientLabs] PRIMARY KEY CLUSTERED 
(
	[LabID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


