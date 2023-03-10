USE [Northwind]
GO
/****** Object:  StoredProcedure [dbo].[pr_GetOrderSummary]    Script Date: 1/19/2023 3:41:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[pr_GetOrderSummary]
    @StartDate DATE,
    @EndDate DATE,
    @EmployeeID INT = NULL,
    @CustomerID Varchar = NULL
AS
BEGIN
    SET NOCOUNT ON;
SELECT
    e.TitleOfCourtesy + ' ' + e.FirstName + ' ' + e.LastName as EmployeeFullName,
    s.CompanyName as [Shipper CompanyName],
    c.CompanyName AS [Customer CompanyName],
	COUNT(o.OrderID) AS NumberOfOrders,
	o.OrderDate as [Date],
    SUM(o.Freight) AS TotalFreightCost,
    COUNT(DISTINCT od.ProductID) AS NumberOfDifferentProducts,
    SUM(od.UnitPrice * od.Quantity) AS TotalOrderValue
FROM Orders o
JOIN Employees e ON o.EmployeeID = e.EmployeeID
JOIN Customers c ON o.CustomerID = c.CustomerID
JOIN Shippers s ON o.ShipVia = s.ShipperID
JOIN [Order Details] od ON o.OrderID = od.OrderID
WHERE 
    o.OrderDate BETWEEN @StartDate AND @EndDate
    AND (@EmployeeID IS NULL OR o.EmployeeID = @EmployeeID)
    AND (@CustomerID IS NULL OR o.CustomerID like '%'+@CustomerID+'%')
GROUP BY
    o.OrderDate,
    e.TitleOfCourtesy,
    e.FirstName,
    e.LastName,
    e.EmployeeID,
    c.CustomerID,
    s.CompanyName,
	c.CompanyName
END