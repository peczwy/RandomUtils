/****
	Bootstrap:
	1) Usuwamy FK z faktu (wymiary nie mają FK (wg. Kimballa i Linsteada)
	2) Usuwamy to co przyrostowe
	3) Tworzymy od nowa
	4) Tworzymy jednorazowo tabelę faktów
*****/

/*
Część 1)
*/
IF OBJECT_ID (N'Customer_FK', N'F') IS NOT NULL 
BEGIN
	ALTER TABLE [SALES_FACT]
	DROP CONSTRAINT Customer_FK;
END

IF OBJECT_ID (N'Date_FK', N'F') IS NOT NULL 
BEGIN
	ALTER TABLE [SALES_FACT]
	DROP CONSTRAINT Date_FK;
END

IF OBJECT_ID (N'Store_FK', N'F') IS NOT NULL 
BEGIN
	ALTER TABLE [SALES_FACT]
	DROP CONSTRAINT Store_FK;
END

/*
Część 2)
*/
IF OBJECT_ID (N'SALES_SNAPSHOT', N'U') IS NOT NULL 
	DROP TABLE [Simple].[dbo].[SALES_SNAPSHOT];
	
IF OBJECT_ID (N'STORE_DIM', N'U') IS NOT NULL 
	DROP TABLE [Simple].[dbo].[STORE_DIM];

IF OBJECT_ID (N'DATE_DIM', N'U') IS NOT NULL
	DROP TABLE [Simple].[dbo].[DATE_DIM];

IF OBJECT_ID (N'CUSTOMER_DIM', N'U') IS NOT NULL 
	DROP TABLE [Simple].[dbo].[CUSTOMER_DIM];

/*
Część 4)
*/
IF OBJECT_ID (N'AUDIT_DIM', N'U') IS NULL 
BEGIN
	CREATE TABLE [dbo].[AUDIT_DIM](
		[ID] INT IDENTITY PRIMARY KEY,
		[DateTo] [DateTime] NOT NULL,
		[StartDate] [DateTime] NOT NULL DEFAULT GETDATE(),
		[RecordCount] [int] NOT NULL DEFAULT 0,
		[Success] [bit] NOT NULL DEFAULT 0
	) ON [PRIMARY]
END

IF OBJECT_ID (N'SALES_FACT', N'U') IS NULL 
BEGIN
	CREATE TABLE [dbo].[SALES_FACT](
		[ID] INT IDENTITY PRIMARY KEY,
		[AuditDim] [int] NOT NULL REFERENCES AUDIT_DIM(ID),
		[CustomerId] [int] NOT NULL,
		[StoreId] [int] NOT NULL,
		[DateId] [int] NOT NULL,
		[OrderQty] [int] NULL,
		[UnitPrice] [money] NULL,
		[TotalValue] [money] NULL,
	) ON [PRIMARY]
END

GO

/****
	Start procesu,
	czyli inicjalizujemy rekord audytu
****/
DECLARE @AUDIT_ID INT;
DECLARE @DATE_FROM DATETIME;
DECLARE @AUDIT_TAB TABLE ( pk INT, datefrom DATETIME);

INSERT INTO [dbo].[AUDIT_DIM]([DateTo])
OUTPUT INSERTED.[ID], INSERTED.DateTo  INTO @AUDIT_TAB
VALUES ((SELECT ISNULL(MAX(DateTo), '2010-01-01') FROM [dbo].[AUDIT_DIM] WHERE Success = 1))

SET @AUDIT_ID = (SELECT MAX(pk) FROM @AUDIT_TAB);
SET @DATE_FROM = (SELECT MAX(datefrom) FROM @AUDIT_TAB);

/****
FACT
****/
SELECT 
	H.CustomerID AS CustomerId,						-- Wymiar klienta
	CAST(H.ShipDate AS DATE) AS ShipDay,			-- Wymiar daty
	C.StoreID AS StoreId,							-- Wymiar sklepu
	SUM(D.OrderQty) AS OrderQty,					-- Miary
	SUM(D.UnitPrice) AS UnitPrice,					-- Miary
	SUM(D.OrderQty * D.UnitPrice) AS TotalValue		-- Miary
INTO 
	[Simple].[dbo].[SALES_SNAPSHOT]
FROM 
	[AdventureWorks2016].[Sales].[SalesOrderHeader] H
	INNER JOIN [AdventureWorks2016].[Sales].[SalesOrderDetail] D ON H.SalesOrderID = D.SalesOrderId
	INNER JOIN [AdventureWorks2016].[Sales].[Customer] C ON C.CustomerID = H.CustomerID
WHERE
	CAST(H.ShipDate AS DATE) > @DATE_FROM 
GROUP BY
	C.StoreID,
	H.CustomerID,			
	H.ShipDate;
	
		
/****
FACT_PREPROCESSING
****/
  ALTER TABLE [Simple].[dbo].[SALES_SNAPSHOT]
  ALTER COLUMN ShipDay DATE NOT NULL;

  UPDATE [Simple].[dbo].[SALES_SNAPSHOT]
  SET [StoreId] = -1
  WHERE StoreId IS NULL;

  ALTER TABLE [Simple].[dbo].[SALES_SNAPSHOT]
  ALTER COLUMN [StoreId] INT NOT NULL;
		
/****
CUSTOMER_DIMENSION
****/
SELECT
	C.CustomerID AS CustomerId,
	CONCAT(P.LastName, ' ', P.FirstName) AS Name,
	P.PersonType AS Type
INTO
	[Simple].[dbo].[CUSTOMER_DIM]
  FROM 
	[AdventureWorks2016].[Sales].[Customer] C
	LEFT JOIN [AdventureWorks2016].[Person].[Person] P ON C.PersonID = P.BusinessEntityID

ALTER TABLE [Simple].[dbo].[CUSTOMER_DIM]
ADD PRIMARY KEY (CustomerId);
	
/****
DATE_DIMENSION
****/

-- Początek
DECLARE @START_DATE DATE;
DECLARE @TEMP DATE;
DECLARE @PK INT;
SET @START_DATE = CONVERT(DATE, '2011-06-07');
SET @TEMP = @START_DATE;
SET @PK = 1;

DECLARE @DATES TABLE( day DATE, pk INT );

-- Kod
WHILE @TEMP <= '2014-07-07'
BEGIN
	INSERT INTO @DATES VALUES(@TEMP, @PK);
	SET @TEMP = DATEADD(DAY,1, @TEMP);
	SET @PK = @PK + 1;
END

SELECT 
	day AS BK,
	pk AS PK,
	day AS DATE_FORMATTED_SHORT,
	YEAR(day) AS YEAR,
	CAST(((MONTH(day) - 0.1) / 3) AS INT) + 1 AS QUARTER,
	MONTH(day) AS MONTH,
	DAY(day) AS DAY 
INTO 
	[Simple].[dbo].[DATE_DIM]
FROM 
	@DATES;
	
ALTER TABLE [Simple].[dbo].[DATE_DIM]
ALTER COLUMN PK INT NOT NULL;

ALTER TABLE [Simple].[dbo].[DATE_DIM]
ADD PRIMARY KEY (PK);

/****
STORE_DIM
****/
SELECT DISTINCT
	S.BusinessEntityID AS BK,
	S.Name
INTO 
	[Simple].[dbo].[STORE_DIM]
FROM 
	[AdventureWorks2016].[Sales].[Customer] C
	INNER JOIN [AdventureWorks2016].[Sales].[Store] S ON C.StoreID = S.BusinessEntityID
ORDER BY 
	S.BusinessEntityID;
	
ALTER TABLE [Simple].[dbo].[STORE_DIM]
ADD PRIMARY KEY (BK);

INSERT INTO [Simple].[dbo].[STORE_DIM]
VALUES(-1,'N/A');



/****
Z powrotem proces:
****/
/*
DODANIE BK DATE DO FAKTU
*/
ALTER TABLE [Simple].[dbo].[SALES_SNAPSHOT]
ADD [DateId] INT NOT NULL DEFAULT -1;

UPDATE [Simple].[dbo].[SALES_SNAPSHOT]
SET [DateId] = (SELECT [PK] FROM [dbo].[DATE_DIM] WHERE [ShipDay] = [BK])

/*
WSADZENIE PRZYROSTU Z SNAPSHOTU DO FAKTU
*/
INSERT INTO [dbo].[SALES_FACT]([AuditDim], [CustomerId],[StoreId],[DateId],[OrderQty],	[UnitPrice],[TotalValue])
SELECT
		@AUDIT_ID 
	  ,[CustomerId]
      ,[StoreId]
      ,[DateId]
      ,[OrderQty]
      ,[UnitPrice]
      ,[TotalValue]
FROM 
	[Simple].[dbo].[SALES_SNAPSHOT]

/**
FOREIGN KEY
**/
ALTER TABLE [SALES_FACT]
ADD CONSTRAINT Customer_FK FOREIGN KEY (CustomerId) REFERENCES CUSTOMER_DIM(CustomerId);

ALTER TABLE [SALES_FACT]
ADD CONSTRAINT Date_FK FOREIGN KEY (DateId) REFERENCES DATE_DIM(PK);

ALTER TABLE [SALES_FACT]
ADD CONSTRAINT Store_FK FOREIGN KEY (StoreId) REFERENCES STORE_DIM(BK);


/*
COMMIT
*/
UPDATE [dbo].[AUDIT_DIM]
SET	
	Success = 1,
	RecordCount = (SELECT COUNT(*) FROM [dbo].[SALES_SNAPSHOT]),
	DateTo = ISNULL((SELECT MAX(ShipDay) FROM [dbo].[SALES_SNAPSHOT]), @DATE_FROM)
WHERE [Id] = @AUDIT_ID