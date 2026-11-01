-- ====================================================================
-- StoryHub - Full Database Backup Script
-- ====================================================================

DECLARE @BackupPath NVARCHAR(500);
DECLARE @BackupFileName NVARCHAR(500);
DECLARE @DatabaseName NVARCHAR(100) = N'StoryHubDb';
DECLARE @Timestamp NVARCHAR(20) = CONVERT(NVARCHAR(20), GETDATE(), 112) + N'_' + REPLACE(CONVERT(NVARCHAR(20), GETDATE(), 108), N':', N'');

-- Cấu hình thư mục lưu backup (đảm bảo dịch vụ SQL Server có quyền ghi)
SET @BackupPath = N'/var/opt/mssql/backup/'; -- Hoặc N'C:\SQLBackups\' trên Windows
SET @BackupFileName = @BackupPath + @DatabaseName + N'_Full_' + @Timestamp + N'.bak';

PRINT N'Bắt đầu sao lưu cơ sở dữ liệu: ' + @DatabaseName;
PRINT N'Đường dẫn tệp backup: ' + @BackupFileName;

BACKUP DATABASE @DatabaseName
TO DISK = @BackupFileName
WITH 
    FORMAT,
    INIT,
    NAME = N'StoryHub-Full Database Backup',
    SKIP,
    NOREWIND,
    NOUNLOAD,
    COMPRESSION,
    CHECKSUM,
    STATS = 10;

-- Kiểm tra tính toàn vẹn của tệp sao lưu vừa tạo
RESTORE VERIFYONLY FROM DISK = @BackupFileName WITH CHECKSUM;

PRINT N'Sao lưu cơ sở dữ liệu StoryHub hoàn tất và đã được xác thực an toàn!';
GO
