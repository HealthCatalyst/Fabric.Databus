USE [master]
GO

/****** Object:  Database [DatabusTest]    Script Date: 9/29/2018 8:23:43 PM ******/
CREATE DATABASE [DatabusTest]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'DatabusTest', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\DATA\DatabusTest.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'DatabusTest_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\DATA\DatabusTest_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO

ALTER DATABASE [DatabusTest] SET COMPATIBILITY_LEVEL = 130
GO