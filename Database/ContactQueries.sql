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
     throw 510000,'Connection faliled',1
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

CREATE TABLE SystemErrors
(
Errorid INT PRIMARY KEY IDENTITY (1,1),
ErrorMessage NVARCHAR(MAX),
ErrorStackTrace NVARCHAR(MAX),
ErrorDate DATETIME DEFAULT GETDATE()
);
GO

CREATE OR ALTER PROCEDURE sp_LogSystemErrors
@ErrorMessage NVARCHAR(MAX),
@ErrorStackTrace NVARCHAR(MAX)
AS 
BEGIN
     SET NOCOUNT ON;
     INSERT INTO SystemErrors(ErrorMessage,ErrorStackTrace)
     VALUES(@ErrorMessage,@ErrorStackTrace)	
END
GO


CREATE OR ALTER PROCEDURE sp_GetAllContacts
    @firstname NVARCHAR(10) = NULL,
    @countryid INT = NULL,
    @searchonlynull BIT = 0,
    @searchbyletter INT = 0,
    @ContactId INT = NULL,
	@LastID int = 0,
	@PageSize int = 20
AS 
BEGIN 
    DECLARE @sql NVARCHAR(MAX);

    SET @sql = N'SELECT TOP (@Psize) ContactID, FirstName, LastName, Email, Phone, Address, CountryID FROM Contacts WHERE ContactID > @Lstid';

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
        SET @sql += N' AND CountryID = @CounID';

    EXEC sp_executesql
        @stmt = @sql,
        @params = N'@FN NVARCHAR(10), @CounID INT, @ConID INT, @Psize INT, @Lstid int',
        @FN = @firstname,
        @CounID = @countryid,
        @ConID = @ContactId,
		@Psize = @Pagesize,
		@LstID = @LastID
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

CREATE OR ALTER FUNCTION dbo.f_CheckContactId(@ContactID int)
RETURNS BIT 
AS
BEGIN
    RETURN (SELECT CASE
	     WHEN EXISTS(SELECT 1 FROM Contacts WHERE ContactID=@ContactID) THEN 1
		 ELSE 0
     END);
END
GO

CREATE OR ALTER PROCEDURE sp_CheckContactId
@ContactId int 
AS
BEGIN
     SELECT dbo.f_CheckContactId(@ContactId) AS Result;
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

IF OBJECT_ID('ContactHistory','u') IS NULL
  BEGIN
   CREATE TABLE ContactHistory
   (  
      LogId INT IDENTITY PRIMARY KEY,
      ContactID int,
      firstname nvarchar(10),
      lastname nvarchar(20),
      email nvarchar(30),
      phone nvarchar(15),
      address nvarchar(50),
      countryid int,
	  ModifaieDate DATETIME DEFAULT GETDATE(),
	  ModifaieBy nvarchar(max),
	  Operation nvarchar(max)
   )

  END
ELSE
  BEGIN
     PRINT 'ITS ALREADY EXISTS'
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

CREATE OR ALTER TRIGGER trg_AfterInsertedContact on Contacts
AFTER INSERT 
AS 
BEGIN
	 INSERT INTO ContactHistory(ContactID,firstname,lastname,email,phone,address,countryid,Operation,ModifaieBy)
	 SELECT ContactID,firstname,lastname,email,phone,address,countryid,'INSERTED',
	 ISNULL(CAST(SESSION_CONTEXT(N'ModifaiedBy')AS nvarchar(50)),SYSTEM_USER) FROM inserted;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertBulkContacts
    @ContactList ContactTableType READONLY,
	@ModifaiedBy NVARCHAR(50)
AS
BEGIN
     SET NOCOUNT ON;
	 SET XACT_ABORT ON;
	 EXEC sp_set_session_context @key=N'ModifaiedBy' ,@value=@ModifaiedBy;

	 DECLARE @InsertedContacts TABLE
	      (
	       ContactID INT,
	       FirstName NVARCHAR(10),
           LastName NVARCHAR(20),
           Email NVARCHAR(20),
           Phone NVARCHAR(10),
           Address NVARCHAR(30),
           CountryID INT
		  )

    INSERT INTO Contacts (FirstName, LastName, Email, Phone, Address, CountryID)
    OUTPUT inserted.ContactID, inserted.FirstName, inserted.LastName, inserted.Email, inserted.Phone, inserted.Address, inserted.CountryID INTO @InsertedContacts

    SELECT FirstName, LastName, Email, Phone, Address, CountryID FROM @ContactList;

	SELECT * FROM @InsertedContacts;
END 
GO

IF NOT EXISTS(SELECT * FROM SYS.types WHERE NAME ='DBO.CONTACTTYPE' AND is_table_type=1)
BEGIN
    CREATE TYPE DBO.CONTACTTYPE AS TABLE
	(
	 ContactID int,
	 firstname nvarchar(10),
	 lastname nvarchar(20),
	 email nvarchar(30),
	 phone nvarchar(15),
	 address nvarchar(50),
	 countryid int
	);
END
GO

CREATE OR ALTER TRIGGER trg_AfterUpdateContact on Contacts
AFTER UPDATE 
AS 
BEGIN
	 INSERT INTO ContactHistory(ContactID,firstname,lastname,email,phone,address,countryid,Operation,ModifaieBy)
	 SELECT ContactID,firstname,lastname,email,phone,address,countryid,'UPDATED',
	 ISNULL(CAST(SESSION_CONTEXT(N'ModifaiedBy') AS nvarchar(50)), SYSTEM_USER) 
	 FROM deleted;

END
GO

CREATE OR ALTER PROCEDURE SP_UpdateBulkContact
@contactlist DBO.CONTACTTYPE readonly,
@ModifaiedBy nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
	SET XACT_ABORT ON;
	EXEC sp_set_session_context @key=N'ModifaiedBy',@value=@ModifaiedBy;

	DECLARE @UpdateContacts DBO.CONTACTTYPE 
    UPDATE Contacts
	       SET FirstName=ISNULL(T.firstname,Contacts.FirstName),
	        LastName=ISNULL(T.LastName,Contacts.LastName),
			Email=ISNULL(T.Email,Contacts.Email),
			Phone=ISNULL(T.Phone,Contacts.Phone),
			Address=ISNULL(T.Address,Contacts.Address),
			CountryID=ISNULL(T.countryid,Contacts.CountryID)

			OUTPUT inserted.* INTO @updatecontacts
			from Contacts 
			inner join @contactlist T on T.ContactID=Contacts.ContactID;

			SELECT * FROM @UpdateContacts;
END
GO

IF NOT EXISTS(SELECT * FROM SYS.types WHERE NAME ='dbo.Deletetype' AND is_table_type=1)
BEGIN
CREATE TYPE dbo.Deletetype AS TABLE
(
 ContactID INT 
)
END
GO

CREATE OR ALTER TRIGGER trg_AfterDeleteContact ON Contacts
After Delete
AS
BEGIN
	 INSERT INTO ContactHistory(ContactID,firstname,lastname,email,phone,address,countryid,Operation,ModifaieBy)
	 SELECT ContactID,firstname,lastname,email,phone,address,countryid,'DELETED',
	 ISNULL(CAST(SESSION_CONTEXT(N'ModifaiedBy')AS nvarchar(50)), SYSTEM_USER )
	 FROM deleted;
END
GO

CREATE OR ALTER PROCEDURE sp_DeleteBulkContacts
@contactlist dbo.Deletetype readonly,
@ModifaiedBy nvarchar(50)
AS
BEGIN
     SET XACT_ABORT ON;
     SET NOCOUNT ON;
     EXEC sp_set_session_context @key = N'ModifaiedBy',@value=@ModifaiedBy;

     DECLARE @DELETEDCONTACTS  TABLE
	 (
	  ContactID int,
      firstname nvarchar(10),
      lastname nvarchar(20),
      email nvarchar(30),
      phone nvarchar(15),
      address nvarchar(50),
      countryid int
	 )
     DELETE C
	 OUTPUT deleted.* into @DeletedContacts
	 FROM Contacts C
	 INNER JOIN @contactlist E ON E.ContactID=C.ContactID
	 WHERE C.ContactID=E.ContactID;

	 SELECT * FROM @DeletedContacts;
END
GO

EXEC sp_GetAllContacts
GO

SELECT * FROM ContactHistory
go

SELECT * FROM SystemErrors ORDER BY Errorid ASC
go