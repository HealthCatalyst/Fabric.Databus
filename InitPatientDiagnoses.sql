USE [DatabusTest]
GO

INSERT INTO [dbo].[PatientDiagnoses]
           ([DiagnosisID]
           ,[PatientID]
           ,[DiagnosisCode]
           ,[DiagnosisDescription])
     VALUES
           (1, 1, 'E11.9', 'Type 2 diabetes mellitus without complications')
GO

INSERT INTO [dbo].[PatientDiagnoses]
           ([DiagnosisID]
           ,[PatientID]
           ,[DiagnosisCode]
           ,[DiagnosisDescription])
     VALUES
           (2, 1, 'I97.130', 'Postprocedural heart failure following cardiac surgery')
GO

INSERT INTO [dbo].[PatientDiagnoses]
           ([DiagnosisID]
           ,[PatientID]
           ,[DiagnosisCode]
           ,[DiagnosisDescription])
     VALUES
           (3, 2, 'I97.3', 'Postprocedural hypertension')
GO

