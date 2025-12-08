-- Wait for SQL Server to be ready
WAITFOR DELAY '00:00:05';
GO

USE master;
GO

-- Create Login 'admin'
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'admin')
BEGIN
    CREATE LOGIN [admin] WITH PASSWORD = 'Password1234';
    PRINT 'Login [admin] created successfully';
END
ELSE
BEGIN
    PRINT 'Login [admin] already exists';
END
GO

-- Create Database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ElasticsearchDemo')
BEGIN
    CREATE DATABASE ElasticsearchDemo;
    PRINT 'Database [ElasticsearchDemo] created successfully';
END
ELSE
BEGIN
    PRINT 'Database [ElasticsearchDemo] already exists';
END
GO

-- Switch to the database
USE ElasticsearchDemo;
GO

-- Create User 'admin'
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'admin')
BEGIN
    CREATE USER [admin] FOR LOGIN [admin];
    PRINT 'User [admin] created successfully';
END
ELSE
BEGIN
    PRINT 'User [admin] already exists';
END
GO

-- Grant db_owner role to admin
ALTER ROLE db_owner ADD MEMBER [admin];
GO

PRINT 'Setup completed: admin user with Password1234';
GO

