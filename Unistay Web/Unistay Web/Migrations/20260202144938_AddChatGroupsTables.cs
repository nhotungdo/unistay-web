using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unistay_Web.Migrations
{
    /// <inheritdoc />
    public partial class AddChatGroupsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatGroups')
                BEGIN
                    CREATE TABLE [ChatGroups] (
                        [Id] int NOT NULL IDENTITY,
                        [Name] nvarchar(max) NOT NULL,
                        [Description] nvarchar(max) NULL,
                        [AvatarUrl] nvarchar(max) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [CreatedBy] nvarchar(450) NOT NULL,
                        CONSTRAINT [PK_ChatGroups] PRIMARY KEY ([Id])
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatGroupMembers')
                BEGIN
                    CREATE TABLE [ChatGroupMembers] (
                        [Id] int NOT NULL IDENTITY,
                        [ChatGroupId] int NOT NULL,
                        [UserId] nvarchar(450) NOT NULL,
                        [Role] int NOT NULL,
                        [JoinedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_ChatGroupMembers] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ChatGroupMembers_ChatGroups_ChatGroupId] FOREIGN KEY ([ChatGroupId]) REFERENCES [ChatGroups] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_ChatGroupMembers_UserProfiles_UserId] FOREIGN KEY ([UserId]) REFERENCES [UserProfiles] ([Id]) ON DELETE CASCADE
                    );
                    
                    CREATE INDEX [IX_ChatGroupMembers_ChatGroupId] ON [ChatGroupMembers] ([ChatGroupId]);
                    CREATE INDEX [IX_ChatGroupMembers_UserId] ON [ChatGroupMembers] ([UserId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatGroupMembers");

            migrationBuilder.DropTable(
                name: "ChatGroups");
        }
    }
}
