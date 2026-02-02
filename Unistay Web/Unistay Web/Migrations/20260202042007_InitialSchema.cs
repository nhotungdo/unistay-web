using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unistay_Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safely create ChatGroups
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ChatGroups]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [ChatGroups] (
                        [Id] int NOT NULL IDENTITY,
                        [Name] nvarchar(max) NULL,
                        CONSTRAINT [PK_ChatGroups] PRIMARY KEY ([Id])
                    );
                END
            ");

            // Safely create ChatGroupMembers
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ChatGroupMembers]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [ChatGroupMembers] (
                        [Id] int NOT NULL IDENTITY,
                        [ChatGroupId] int NOT NULL,
                        [UserId] nvarchar(max) NOT NULL,
                        [Role] int NOT NULL,
                        CONSTRAINT [PK_ChatGroupMembers] PRIMARY KEY ([Id])
                    );
                END
            ");

            // Safely create MessageAttachments (Simplified schema based on earlier view)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[MessageAttachments]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [MessageAttachments] (
                        [Id] int NOT NULL IDENTITY,
                        [MessageId] int NOT NULL,
                        [FileName] nvarchar(max) NULL,
                        [FilePath] nvarchar(max) NULL,
                        [FileType] nvarchar(max) NULL,
                        [FileSize] bigint NOT NULL,
                        [ThumbnailPath] nvarchar(max) NULL,
                        [UploadedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_MessageAttachments] PRIMARY KEY ([Id])
                    );
                END
            ");
            
            // Safely create MessageReports
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[MessageReports]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [MessageReports] (
                        [Id] int NOT NULL IDENTITY,
                        [ReporterId] nvarchar(450) NULL,
                        [MessageId] int NOT NULL,
                        [Reason] int NOT NULL,
                        [Description] nvarchar(max) NULL,
                        [Status] int NOT NULL,
                        [ReportedAt] datetime2 NOT NULL,
                        [ResolvedAt] datetime2 NULL,
                        [AdminNote] nvarchar(max) NULL,
                        CONSTRAINT [PK_MessageReports] PRIMARY KEY ([Id])
                    );
                END
            ");

             // Safely create BlockedUsers
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[BlockedUsers]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [BlockedUsers] (
                        [Id] int NOT NULL IDENTITY,
                        [BlockerId] nvarchar(450) NULL,
                        [BlockedUserId] nvarchar(450) NULL,
                        [Reason] nvarchar(max) NULL,
                        [BlockedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_BlockedUsers] PRIMARY KEY ([Id])
                    );
                END
            ");


            // Ensure Messages table exists or alter it
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Messages]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Messages] (
                        [Id] int NOT NULL IDENTITY,
                        [SenderId] nvarchar(450) NULL,
                        [ReceiverId] nvarchar(450) NULL,
                        [ChatGroupId] int NULL,
                        [Content] nvarchar(max) NULL,
                        [Type] int NOT NULL,
                        [Status] int NOT NULL,
                        [IsEncrypted] bit NOT NULL,
                        [IsEdited] bit NOT NULL,
                        [EditedAt] datetime2 NULL,
                        [IsDeleted] bit NOT NULL,
                        [DeletedAt] datetime2 NULL,
                        [ReplyToMessageId] int NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [DeliveredAt] datetime2 NULL,
                        [SeenAt] datetime2 NULL,
                        CONSTRAINT [PK_Messages] PRIMARY KEY ([Id])
                    );
                END
                ELSE
                BEGIN
                     -- Table exists, check for missing columns
                     IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'ChatGroupId' AND Object_ID = Object_ID(N'Messages'))
                     BEGIN
                         ALTER TABLE [Messages] ADD [ChatGroupId] int NULL;
                     END
                     
                     IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'EditedAt' AND Object_ID = Object_ID(N'Messages'))
                     BEGIN
                         ALTER TABLE [Messages] ADD [EditedAt] datetime2 NULL;
                     END
                     
                     IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsEdited' AND Object_ID = Object_ID(N'Messages'))
                     BEGIN
                         ALTER TABLE [Messages] ADD [IsEdited] bit NOT NULL DEFAULT 0;
                     END
                END
            ");

            // Constraints for Messages (Idempotent)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_Messages_ChatGroups_ChatGroupId]') AND parent_object_id = OBJECT_ID(N'[Messages]'))
                BEGIN
                    ALTER TABLE [Messages] ADD CONSTRAINT [FK_Messages_ChatGroups_ChatGroupId] FOREIGN KEY ([ChatGroupId]) REFERENCES [ChatGroups] ([Id]);
                END
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Messages_ChatGroupId' AND object_id = OBJECT_ID('Messages'))
                BEGIN
                    CREATE INDEX [IX_Messages_ChatGroupId] ON [Messages] ([ChatGroupId]);
                END
            ");
            
            // Note: Omitting UserProfiles, Connections, etc. creation here as they are assumed to exist. 
            // If they are missing, this migration does not recreate them, but the Snapshot still defines them.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Safe to leave empty or add drops if needed. avoiding accidental data loss.
        }
    }
}
