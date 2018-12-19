CREATE SCHEMA HCOSText;

CREATE TABLE [HCOSText].[DataBASE](	[BindingID] [int] NOT NULL,	[BindingNM] [varchar](255) NOT NULL,	[LastLoadDTS] [datetime2](7) NOT NULL,	[TextKEY] [varchar](100) NULL,	[PatientKEY] [varchar](100) NULL,	[VisitKEY] [varchar](100) NULL,	[data] [varchar](max) NULL,	[base64_data] [varchar](max) NULL,	[data_format] [varchar](255) NULL,	[root] [varchar](255) NOT NULL,	[extension] [varchar](255) NOT NULL,	[extension_suffix] [int] NULL,	[source_last_modified_at] [nvarchar](4000) NULL,	[source_versioned_at] [nvarchar](4000) NULL,	[data_precedence] [varchar](255) NOT NULL);
CREATE VIEW [HCOSText].[Data] AS SELECT [BindingID], [BindingNM], [LastLoadDTS], [TextKEY], [PatientKEY], [VisitKEY], [data], [base64_data], [data_format], [root], [extension], [extension_suffix], [source_last_modified_at], [source_versioned_at], [data_precedence] FROM [HCOSText].[DataBASE];

CREATE TABLE [HCOSText].[PatientBASE](	[BindingID] [int] NOT NULL,	[BindingNM] [varchar](255) NOT NULL,	[LastLoadDTS] [datetime2](7) NOT NULL,	[PatientKEY] [varchar](100) NULL,	[root] [varchar](255) NOT NULL,	[extension] [varchar](255) NULL,	[last_name] [varchar](255) NULL,	[first_name] [varchar](255) NULL,	[middle_name] [varchar](1000) NULL,	[gender] [varchar](255) NOT NULL,	[date_of_birth] [nvarchar](4000) NULL)
CREATE VIEW [HCOSText].[Patient] AS SELECT [BindingID], [BindingNM], [LastLoadDTS], [PatientKEY], [root], [extension], [last_name], [first_name], [middle_name], [gender], [date_of_birth] FROM [HCOSText].[PatientBASE]

CREATE TABLE [HCOSText].[VisitBASE](	[BindingID] [int] NOT NULL,	[BindingNM] [varchar](255) NOT NULL,	[LastLoadDTS] [datetime2](7) NOT NULL,	[VisitKEY] [varchar](100) NULL,	[VisitFacilityKEY] [varchar](100) NULL,	[VisitPeopleKEY] [varchar](100) NULL,	[root] [varchar](255) NOT NULL,	[extension] [varchar](255) NULL,	[admitted_at] [nvarchar](4000) NULL,	[discharged_at] [nvarchar](4000) NULL)
CREATE VIEW [HCOSText].[Visit] AS SELECT [BindingID], [BindingNM], [LastLoadDTS], [VisitKEY], [VisitFacilityKEY], [VisitPeopleKEY], [root], [extension], [admitted_at], [discharged_at] FROM [HCOSText].[VisitBASE]

CREATE TABLE [HCOSText].[VisitFacilityBASE](	[BindingID] [int] NOT NULL,	[BindingNM] [varchar](255) NOT NULL,	[LastLoadDTS] [datetime2](7) NOT NULL,	[VisitFacilityKEY] [varchar](100) NULL,	[root] [varchar](255) NULL,	[extension] [varchar](255) NULL)
CREATE VIEW [HCOSText].[VisitFacility] AS SELECT [BindingID], [BindingNM], [LastLoadDTS], [VisitFacilityKEY], [root], [extension] FROM [HCOSText].[VisitFacilityBASE]

CREATE TABLE [HCOSText].[VisitPeopleBASE](	[BindingID] [int] NOT NULL,	[BindingNM] [varchar](255) NOT NULL,	[LastLoadDTS] [datetime2](7) NOT NULL,	[VisitPeopleKEY] [varchar](100) NULL,	[role] [varchar](255) NOT NULL,	[root] [varchar](255) NULL,	[extension] [varchar](255) NULL,	[last_name] [varchar](255) NULL,	[first_name] [varchar](255) NULL,	[middle_name] [varchar](255) NULL)
CREATE VIEW [HCOSText].[VisitPeople] AS SELECT [BindingID], [BindingNM], [LastLoadDTS], [VisitPeopleKEY], [role], [root], [extension], [last_name], [first_name], [middle_name] FROM [HCOSText].[VisitPeopleBASE]

CREATE TABLE [HCOSText].[DocumentBASE](	[BindingID] [int] NOT NULL,	[BindingNM] [varchar](255) NOT NULL,	[LastLoadDTS] [datetime2](7) NOT NULL,	[TextKEY] [varchar](100) NULL,	[root] [varchar](255) NULL,	[extension] [varchar](255) NOT NULL,	[status] [varchar](255) NOT NULL,	[confidentiality_code] [varchar](255) NOT NULL,	[source_created_at] [nvarchar](4000) NULL,	[source_updated_at] [nvarchar](4000) NULL,	[type_root] [varchar](255) NULL,	[type_extension] [varchar](255) NULL,	[type_description] [varchar](255) NULL,	[date_of_service] [nvarchar](4000) NULL)
CREATE VIEW [HCOSText].[Document] AS SELECT [BindingID], [BindingNM], [LastLoadDTS], [TextKEY], [root], [extension], [status], [confidentiality_code], [source_created_at], [source_updated_at], [type_root], [type_extension], [type_description], [date_of_service] FROM [HCOSText].[DocumentBASE]

CREATE TABLE [HCOSText].[DocumentPeopleBASE](	[BindingID] [int] NOT NULL,	[BindingNM] [varchar](255) NOT NULL,	[LastLoadDTS] [datetime2](7) NOT NULL,	[TextKEY] [varchar](100) NULL,	[root] [varchar](255) NOT NULL,	[extension] [varchar](255) NOT NULL,	[role] [varchar](255) NOT NULL,	[last_name] [varchar](255) NULL,	[first_name] [varchar](255) NULL,	[middle_name] [varchar](255) NULL)
CREATE VIEW [HCOSText].[DocumentPeople] AS SELECT [BindingID], [BindingNM], [LastLoadDTS], [TextKEY], [root], [extension], [role], [last_name], [first_name], [middle_name] FROM [HCOSText].[DocumentPeopleBASE]

INSERT INTO Text (TextID, PatientID, TextTXT) values ('1', 9001, 'This is my first note')
INSERT INTO Text (TextID, PatientID, TextTXT) values ('2', 9002, 'This is my second note')
INSERT INTO Text (TextID, PatientID, TextTXT) values ('3', 9003, 'This is my third note')
INSERT INTO Text (TextID, PatientID, TextTXT) values ('4', 9004, 'This is my fourth note')
INSERT INTO Text (TextID, PatientID, TextTXT) values ('5', 9005, 'This is my fifth note')
INSERT INTO Patients (TextID, PatientID, PatientLastNM) values ('1', 9001, 'Jones')
INSERT INTO Patients (TextID, PatientID, PatientLastNM) values ('2', 9002, 'Smith')
INSERT INTO Patients (TextID, PatientID, PatientLastNM) values ('3', 9003, 'Forbes')
INSERT INTO Patients (TextID, PatientID, PatientLastNM) values ('4', 9004, 'Sandovsky')