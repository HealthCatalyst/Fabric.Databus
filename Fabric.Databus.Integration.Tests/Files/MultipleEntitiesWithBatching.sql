﻿CREATE TABLE Text (TextID varchar(64), PatientID int, TextTXT varchar(255))
CREATE TABLE Patients (TextID varchar(64), PatientID int, PatientLastNM varchar(255))
INSERT INTO Text (TextID, PatientID, TextTXT) values ('1', 9001, 'This is my first note')
INSERT INTO Text (TextID, PatientID, TextTXT) values ('2', 9002, 'This is my second note')
INSERT INTO Text (TextID, PatientID, TextTXT) values ('3', 9003, 'This is my third note')
INSERT INTO Text (TextID, PatientID, TextTXT) values ('4', 9004, 'This is my fourth note')
INSERT INTO Text (TextID, PatientID, TextTXT) values ('5', 9005, 'This is my fifth note')
INSERT INTO Patients (TextID, PatientID, PatientLastNM) values ('1', 9001, 'Jones')
INSERT INTO Patients (TextID, PatientID, PatientLastNM) values ('2', 9002, 'Smith')
INSERT INTO Patients (TextID, PatientID, PatientLastNM) values ('3', 9003, 'Forbes')
INSERT INTO Patients (TextID, PatientID, PatientLastNM) values ('4', 9004, 'Sandovsky')