using Microsoft.EntityFrameworkCore;
using Unistay_Web.Data;

namespace Unistay_Web.Data
{
    public static class DatabaseSeeder
    {
        public static void EnsureTablesExist(ApplicationDbContext context)
        {
            try
            {
                // RoommateProfiles
                context.Database.ExecuteSqlRaw(@"
                    IF OBJECT_ID('RoommateProfiles', 'U') IS NULL
                    BEGIN
                        CREATE TABLE [RoommateProfiles] (
                            [Id] int NOT NULL IDENTITY,
                            [UserId] nvarchar(450) NOT NULL,
                            [Gender] nvarchar(max) NULL,
                            [Budget] decimal(18,2) NOT NULL,
                            [PreferredArea] nvarchar(max) NULL,
                            [MoveInDate] datetime2 NULL,
                            [Habits] nvarchar(max) NULL,
                            [Status] nvarchar(max) NOT NULL,
                            [CreatedAt] datetime2 NOT NULL,
                            [UpdatedAt] datetime2 NULL,
                            CONSTRAINT [PK_RoommateProfiles] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_RoommateProfiles_UserProfiles_UserId] FOREIGN KEY ([UserId]) REFERENCES [UserProfiles] ([Id]) ON DELETE CASCADE
                        );
                        CREATE INDEX [IX_RoommateProfiles_UserId] ON [RoommateProfiles] ([UserId]);
                    END
                ");

                // Connections
                context.Database.ExecuteSqlRaw(@"
                    IF OBJECT_ID('Connections', 'U') IS NULL
                    BEGIN
                        CREATE TABLE [Connections] (
                            [Id] int NOT NULL IDENTITY,
                            [RequesterId] nvarchar(450) NOT NULL,
                            [AddresseeId] nvarchar(450) NOT NULL,
                            [Status] int NOT NULL,
                            [CreatedAt] datetime2 NOT NULL,
                            [UpdatedAt] datetime2 NULL,
                            CONSTRAINT [PK_Connections] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_Connections_UserProfiles_RequesterId] FOREIGN KEY ([RequesterId]) REFERENCES [UserProfiles] ([Id]) ON DELETE NO ACTION,
                            CONSTRAINT [FK_Connections_UserProfiles_AddresseeId] FOREIGN KEY ([AddresseeId]) REFERENCES [UserProfiles] ([Id]) ON DELETE NO ACTION
                        );
                        CREATE INDEX [IX_Connections_RequesterId] ON [Connections] ([RequesterId]);
                        CREATE INDEX [IX_Connections_AddresseeId] ON [Connections] ([AddresseeId]);
                    END
                ");
                
                // Conversations (Missing from failing migration?)
                 context.Database.ExecuteSqlRaw(@"
                    IF OBJECT_ID('Conversations', 'U') IS NULL
                    BEGIN
                        CREATE TABLE [Conversations] (
                            [Id] int NOT NULL IDENTITY,
                            [Name] nvarchar(max) NULL,
                            [CreatedAt] datetime2 NOT NULL,
                            [UpdatedAt] datetime2 NOT NULL,
                            CONSTRAINT [PK_Conversations] PRIMARY KEY ([Id])
                        );
                    END
                ");

                // UserConversations
                context.Database.ExecuteSqlRaw(@"
                    IF OBJECT_ID('UserConversations', 'U') IS NULL
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
                        CREATE INDEX [IX_UserConversations_ConversationId] ON [UserConversations] ([ConversationId]);
                        CREATE INDEX [IX_UserConversations_UserId] ON [UserConversations] ([UserId]);
                    END
                ");

                // Mark migration as applied to prevent future conflicts
                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260127154657_AddConnectionsTableOnly')
                    BEGIN
                        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                        VALUES ('20260127154657_AddConnectionsTableOnly', '8.0.0');
                    END
                ");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
                // Continue, relying on existing tables if any
            }
        }
    }
}
