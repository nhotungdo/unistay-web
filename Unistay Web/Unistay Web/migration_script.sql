IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000000_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240101000000_Initial', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000001_AddCoverPhoto'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [CoverPhotoUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000001_AddCoverPhoto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240101000001_AddCoverPhoto', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000002_AddOnboardingFields'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [CompatibilityAnalysis] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000002_AddOnboardingFields'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [DateOfBirth] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000002_AddOnboardingFields'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [HouseNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000002_AddOnboardingFields'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [IsAddressVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000002_AddOnboardingFields'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [IsOnboardingComplete] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000002_AddOnboardingFields'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [LandlordVerificationStatus] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000002_AddOnboardingFields'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [OnboardingStep] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000002_AddOnboardingFields'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [StreetName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000002_AddOnboardingFields'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [ZodiacSign] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000002_AddOnboardingFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240101000002_AddOnboardingFields', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [AverageRating] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [Bio] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [BlockReason] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [BlockedDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [City] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [District] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [EmailVerifiedDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [IdCardNumber] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [IdVerificationDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [IsBlocked] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [IsIdVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [LastLoginAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [LastLoginIp] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [Latitude] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [Longitude] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [NotificationEmailEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [NotificationPushEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [NotificationSmsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [PhoneVerifiedDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [SuccessfulBookings] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [TotalListings] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [TotalReviews] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    ALTER TABLE [UserProfiles] ADD [Ward] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    CREATE TABLE [ActivityHistories] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ActivityType] nvarchar(100) NOT NULL,
        [ActivityDate] datetime2 NOT NULL,
        [Description] nvarchar(500) NULL,
        [RelatedEntity] nvarchar(200) NULL,
        [RelatedEntityType] nvarchar(50) NULL,
        [IpAddress] nvarchar(45) NULL,
        [IsPublic] bit NOT NULL,
        CONSTRAINT [PK_ActivityHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ActivityHistories_UserProfiles_UserId] FOREIGN KEY ([UserId]) REFERENCES [UserProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    CREATE TABLE [LoginHistories] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [LoginTime] datetime2 NOT NULL,
        [LogoutTime] datetime2 NULL,
        [IpAddress] nvarchar(45) NULL,
        [UserAgent] nvarchar(500) NULL,
        [Browser] nvarchar(100) NULL,
        [OperatingSystem] nvarchar(100) NULL,
        [DeviceType] nvarchar(100) NULL,
        [IsSuccessful] bit NOT NULL,
        [FailureReason] nvarchar(500) NULL,
        [AuthenticationMethod] nvarchar(max) NULL,
        CONSTRAINT [PK_LoginHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LoginHistories_UserProfiles_UserId] FOREIGN KEY ([UserId]) REFERENCES [UserProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    CREATE INDEX [IX_ActivityHistories_UserId] ON [ActivityHistories] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    CREATE INDEX [IX_LoginHistories_UserId] ON [LoginHistories] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240101000003_UserProfileEnhancement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240101000003_UserProfileEnhancement', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Messages]') AND [c].[name] = N'SenderId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Messages] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Messages] ALTER COLUMN [SenderId] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Messages]') AND [c].[name] = N'ReceiverId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Messages] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Messages] ALTER COLUMN [ReceiverId] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    ALTER TABLE [Messages] ADD [ConversationId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    ALTER TABLE [Messages] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    ALTER TABLE [Messages] ADD [Type] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    CREATE TABLE [Connections] (
        [Id] int NOT NULL IDENTITY,
        [RequesterId] nvarchar(450) NOT NULL,
        [AddresseeId] nvarchar(450) NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Connections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Connections_UserProfiles_AddresseeId] FOREIGN KEY ([AddresseeId]) REFERENCES [UserProfiles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Connections_UserProfiles_RequesterId] FOREIGN KEY ([RequesterId]) REFERENCES [UserProfiles] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    CREATE TABLE [Conversations] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Conversations] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    CREATE TABLE [UserConversations] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ConversationId] int NOT NULL,
        [JoinedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_UserConversations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserConversations_Conversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserConversations_UserProfiles_UserId] FOREIGN KEY ([UserId]) REFERENCES [UserProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    CREATE INDEX [IX_Messages_ConversationId] ON [Messages] ([ConversationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    CREATE INDEX [IX_Messages_SenderId] ON [Messages] ([SenderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    CREATE INDEX [IX_Connections_AddresseeId] ON [Connections] ([AddresseeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    CREATE INDEX [IX_Connections_RequesterId] ON [Connections] ([RequesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    CREATE INDEX [IX_UserConversations_ConversationId] ON [UserConversations] ([ConversationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    CREATE INDEX [IX_UserConversations_UserId] ON [UserConversations] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    ALTER TABLE [Messages] ADD CONSTRAINT [FK_Messages_Conversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    ALTER TABLE [Messages] ADD CONSTRAINT [FK_Messages_UserProfiles_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [UserProfiles] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260125162407_AddConnectionsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260125162407_AddConnectionsTable', N'8.0.0');
END;
GO

COMMIT;
GO

