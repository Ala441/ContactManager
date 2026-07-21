IF DB_ID('Contactsdb1')IS NULL
BEGIN
    CREATE DATABASE Contactsdb1;
END
ELSE
BEGIN
     PRINT 'Database Already Exists'
END
GO

USE Contactsdb1;
GO

IF DB_NAME()<>'Contactsdb1'
begin
     throw 510000,'Conection fals',1
	 return
END
GO

IF OBJECT_ID('Countries','u') IS NULL
BEGIN
    CREATE TABLE countries (
	CountryID INT PRIMARY KEY,
	CountryName NVARCHAR(50)
	);
END
GO

IF OBJECT_ID('Contacts','u')IS NULL
BEGIN
    CREATE TABLE Contacts (
        ContactID INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(10),
        LastName NVARCHAR(25),
        Email NVARCHAR(30),
        Phone NVARCHAR(15),
        Address NVARCHAR(30),
        CountryID INT REFERENCES Countries(CountryID)
    );
END
GO

IF NOT EXISTS (SELECT * FROM Countries)
BEGIN
    INSERT INTO Countries (CountryID, CountryName) 
	VALUES (1, 'Japan'), (2, 'Saudi Arabia'), (3, 'Philippines'), (4, 'Thailand'), (5, 'South Korea');
END
GO

IF NOT EXISTS(SELECT * FROM Contacts)
BEGIN
    INSERT INTO Contacts (FirstName, LastName, Email, Phone, Address, CountryID)
	VALUES ('Ali', 'Mohammed', 'ali@test.com', '123456', 'Kobe', 1),
           ('Jane', 'Doe', 'jane@test.com', '654321', 'Tokyo', 4),
		   ('Michael', 'Johnson', 'michaeljohnson@example.com', '987654321', 'Kyoto', 3),
		   ('Ala', 'Moqbel', 'alosh@test.com', '070732199266', 'Osaka', 2);
END
GO

CREATE OR ALTER PROCEDURE sp_GetAllContacts
    @firstname NVARCHAR(10) = NULL,
    @countryid INT = NULL,
    @searchonlynull BIT = 0,
    @searchbyletter INT = 0,
    @ContactId INT = NULL
AS 
BEGIN 
    DECLARE @sql NVARCHAR(MAX);

    SET @sql = N'SELECT ContactID, FirstName, LastName, Email, Phone, Address, CountryID FROM Contacts WHERE 1=1';

    IF @searchonlynull = 1
        SET @sql += N' AND (FirstName IS NULL OR CountryID IS NULL)';
    ELSE IF @searchbyletter = 1 AND @firstname IS NOT NULL
        SET @sql += N' AND FirstName LIKE @FN + ''%''';
    ELSE IF @searchbyletter = 2 AND @firstname IS NOT NULL
        SET @sql += N' AND FirstName LIKE ''%'' + @FN';
    ELSE IF @searchbyletter = 3 AND @firstname IS NOT NULL
        SET @sql += N' AND FirstName LIKE ''%'' + @FN + ''%''';
    ELSE
    BEGIN
        IF @firstname IS NOT NULL 
            SET @sql += N' AND FirstName = @FN';
    END

    IF @ContactId IS NOT NULL
        SET @sql += N' AND ContactID = @ConID';

    IF @countryid IS NOT NULL
        SET @sql += N' AND CountryID = @CouID';

    EXEC sp_executesql
        @stmt = @sql,
        @params = N'@FN NVARCHAR(10), @CouID INT, @ConID INT',
        @FN = @firstname,
        @CouID = @countryid,
        @ConID = @ContactId;
END
GO 

CREATE OR ALTER PROCEDURE sp_GetFirstname
    @ContactId INT = NULL
AS
BEGIN
    SELECT FirstName FROM Contacts WHERE ContactID = @ContactId;
END
GO

CREATE OR ALTER PROCEDURE sp_FindContactByID
    @ContactID INT = NULL
AS 
BEGIN
    SELECT ContactID, FirstName, LastName, Email, Phone, Address, CountryID 
    FROM Contacts 
    WHERE ContactID = @ContactID;
END
GO

CREATE OR ALTER FUNCTION dbo.f_CheckCountries(@countryid INT)
RETURNS BIT
AS 
BEGIN
    RETURN (SELECT CASE
        WHEN EXISTS(SELECT 1 FROM Countries WHERE CountryID = @countryid) THEN 1
        ELSE 0
    END);
END
GO

CREATE OR ALTER PROCEDURE sp_CheckCountryExists
    @countryid INT
AS
BEGIN
    SELECT dbo.f_CheckCountries(@countryid) AS Result;
END
GO  

IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'ContactTableType' AND is_table_type = 1)
BEGIN
    CREATE TYPE ContactTableType AS TABLE 
    (
        FirstName NVARCHAR(10),
        LastName NVARCHAR(20),
        Email NVARCHAR(20),
        Phone NVARCHAR(10),
        Address NVARCHAR(30),
        CountryID INT
    )
END
GO

CREATE OR ALTER PROCEDURE sp_InsertBulkContacts
    @ContactList ContactTableType READONLY
AS
BEGIN
    INSERT INTO Contacts (FirstName, LastName, Email, Phone, Address, CountryID)
    OUTPUT inserted.ContactID, inserted.FirstName, inserted.LastName, inserted.Email, inserted.Phone, inserted.Address, inserted.CountryID
    SELECT FirstName, LastName, Email, Phone, Address, CountryID FROM @ContactList;
END 
GO