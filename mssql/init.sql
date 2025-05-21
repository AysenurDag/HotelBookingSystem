-- Veritabanını oluştur
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AuthUserDb')
BEGIN
    CREATE DATABASE AuthUserDb;
END
GO

-- Kullanıcıyı oluştur (eğer ihtiyaç varsa)
IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'LastUser')
BEGIN
    CREATE LOGIN LastUser WITH PASSWORD = 'Aut.Hen.Tication123!';
END
GO

-- Kullanıcıyı veritabanına ekle
USE AuthUserDb;
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'LastUser')
BEGIN
    CREATE USER LastUser FOR LOGIN LastUser;
    EXEC sp_addrolemember N'db_owner', N'LastUser';
END
GO
