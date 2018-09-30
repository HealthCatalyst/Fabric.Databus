USE [DatabusTest]
GO

INSERT INTO [dbo].[Patients]
           ([PatientID]
           ,[PatientLastNM]
           ,[PatientFirstNM]
           ,[DateOfBirth])
     VALUES
           (1, 'James', 'Rick', '10/2/1950')
GO

INSERT INTO [dbo].[Patients]
           ([PatientID]
           ,[PatientLastNM]
           ,[PatientFirstNM]
           ,[DateOfBirth])
     VALUES
           (2, 'Metcalf', 'Bob', '5/6/1945')
GO
