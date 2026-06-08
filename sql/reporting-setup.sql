CREATE OR ALTER VIEW dbo.vw_sales_summary AS
SELECT
    YEAR(o.OrderDate) AS [Year],
    MONTH(o.OrderDate) AS [Month],
    c.Region,
    p.Category,
    SUM(o.TotalAmount) AS TotalSales,
    SUM(CASE WHEN o.TotalAmount < 1000 THEN o.TotalAmount ELSE 0 END) AS MinorSales,
    COUNT(*) AS OrderCount
FROM dbo.Orders o
JOIN dbo.Customers c ON o.CustomerId = c.Id
JOIN dbo.Products p ON o.ProductId = p.Id
GROUP BY
    YEAR(o.OrderDate),
    MONTH(o.OrderDate),
    c.Region,
    p.Category;
GO

-- Run this section with a strong password in a secured environment.
-- CREATE LOGIN AiReporterLogin WITH PASSWORD = 'StrongPasswordHere';
-- CREATE USER AiReporterUser FOR LOGIN AiReporterLogin;
-- GRANT SELECT ON dbo.vw_sales_summary TO AiReporterUser;
