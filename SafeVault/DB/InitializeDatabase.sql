-- Create the Users table if it does not exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Username] NVARCHAR(50) NOT NULL UNIQUE,
        [Password] NVARCHAR(100) NOT NULL, -- Store BCrypt hash here
        [Email] NVARCHAR(100) NOT NULL UNIQUE,
        [Role] NVARCHAR(20) NOT NULL DEFAULT 'User',
        [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
    
    -- Create admin user with BCrypt hashed password (password: Admin@123)
    -- Note: In production, use a more secure password and keep it confidential
    INSERT INTO [dbo].[Users] ([Username], [Password], [Email], [Role])
    VALUES ('admin', '$2a$12$I8oB9hUUvMiz.J8ahaCIEO/WczLFDMZLhPUkM0YGKdyTCkCDJpzVq', 'admin@safevault.com', 'Admin');
    
    -- Create a regular user with BCrypt hashed password (password: User@123)
    INSERT INTO [dbo].[Users] ([Username], [Password], [Email], [Role])
    VALUES ('user', '$2a$12$cdBJpDg0TIErFbfj8t7nIOBWdNnc5nPbBzxHG9LckyXdwwGW2qlpq', 'user@safevault.com', 'User');
    
    PRINT 'Users table created and seeded with default users';
END
ELSE
BEGIN
    PRINT 'Users table already exists';
END
