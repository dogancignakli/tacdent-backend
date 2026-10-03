:on error exit
SET XACT_ABORT ON;
SET NOCOUNT ON;
BEGIN TRANSACTION;

UPDATE dbo.Services SET PriceFromTry = 1000.00, DurationMinutes = 30, UpdatedAt = SYSUTCDATETIME() WHERE Id = 1;
UPDATE dbo.Services SET PriceFromTry = 5000.00, PriceFromEur = 120.00, UpdatedAt = SYSUTCDATETIME() WHERE Id = 2;
UPDATE dbo.Services SET PriceFromTry = 20000.00, PriceFromEur = 600.00, UpdatedAt = SYSUTCDATETIME() WHERE Id = 3;
UPDATE dbo.Services SET PriceFromTry = 2000.00, UpdatedAt = SYSUTCDATETIME() WHERE Id = 5;
DELETE FROM dbo.Services
WHERE Id = 4 AND NOT EXISTS (SELECT 1 FROM dbo.Appointments WHERE ServiceId = 4);

IF (SELECT COUNT(*) FROM dbo.Services) <> 4
    THROW 50001, 'Beklenmeyen servis sayisi; degisiklik geri alindi.', 1;

COMMIT;
SELECT Id, NameTr, PriceFromTry, PriceFromEur, DurationMinutes, IsActive, DisplayOrder
FROM dbo.Services ORDER BY DisplayOrder;
