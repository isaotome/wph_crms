USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[TestMethod]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[TestMethod]
AS
BEGIN
INSERT INTO dbo.TestTable VALUES('001','123')
END
GO
