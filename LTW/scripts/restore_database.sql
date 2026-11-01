-- ====================================================================
-- StoryHub - Database Restore Script
-- ====================================================================

USE master;
GO

DECLARE @DatabaseName NVARCHAR(100) = N'StoryHubDb';
DECLARE @BackupFile NVARCHAR(500) = N'/var/opt/mssql/backup/StoryHubDb_Full.bak'; -- Hoặc đường dẫn Windows

-- 1. Đóng toàn bộ các kết nối đang mở vào Database
ALTER DATABASE [StoryHubDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- 2. Kiểm tra danh sách Logical Name của các file bên trong bản backup
RESTORE FILELISTONLY FROM DISK = N'/var/opt/mssql/backup/StoryHubDb_Full.bak';
GO

-- 3. Tiến hành khôi phục cơ sở dữ liệu và chuyển vị trí file (nếu cần)
RESTORE DATABASE [StoryHubDb]
FROM DISK = N'/var/opt/mssql/backup/StoryHubDb_Full.bak'
WITH 
    REPLACE,
    RECOVERY,
    MOVE N'StoryHubDb' TO N'/var/opt/mssql/data/StoryHubDb.mdf',
    MOVE N'StoryHubDb_log' TO N'/var/opt/mssql/data/StoryHubDb_log.ldf',
    STATS = 10;
GO

-- 4. Đặt lại Database về chế độ MULTI_USER cho người dùng truy cập
ALTER DATABASE [StoryHubDb] SET MULTI_USER;
GO

PRINT N'Khôi phục cơ sở dữ liệu StoryHub hoàn tất thành công!';
GO
