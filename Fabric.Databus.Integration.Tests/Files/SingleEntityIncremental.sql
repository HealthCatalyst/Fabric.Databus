CREATE TABLE Text (TextID varchar(64), PatientID int, TextTXT varchar(255), LastLoadDTS DATETIME2);
INSERT INTO Text (TextID, PatientID, TextTXT, LastLoadDTS) values ('1', 9001, 'This is my first note', '1/1/2017');
INSERT INTO Text (TextID, PatientID, TextTXT, LastLoadDTS) values ('2', 9002, 'This is my second note', '1/7/2017');
