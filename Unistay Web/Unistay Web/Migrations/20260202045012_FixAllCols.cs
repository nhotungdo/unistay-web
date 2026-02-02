using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unistay_Web.Migrations
{
    /// <inheritdoc />
    public partial class FixAllCols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safely add all potentially missing columns to Messages table
            migrationBuilder.Sql(@"
                -- Ensure Type column
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'Type')
                BEGIN
                    ALTER TABLE [Messages] ADD [Type] int NOT NULL DEFAULT 0;
                END

                -- Ensure Status column
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'Status')
                BEGIN
                    ALTER TABLE [Messages] ADD [Status] int NOT NULL DEFAULT 0;
                END

                -- Ensure IsEncrypted column
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'IsEncrypted')
                BEGIN
                    ALTER TABLE [Messages] ADD [IsEncrypted] bit NOT NULL DEFAULT 0;
                END

                -- Ensure IsDeleted column
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'IsDeleted')
                BEGIN
                    ALTER TABLE [Messages] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
                END

                -- Ensure DeletedAt column
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'DeletedAt')
                BEGIN
                    ALTER TABLE [Messages] ADD [DeletedAt] datetime2 NULL;
                END

                -- Ensure ReplyToMessageId column
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'ReplyToMessageId')
                BEGIN
                    ALTER TABLE [Messages] ADD [ReplyToMessageId] int NULL;
                END

                -- Ensure CreatedAt column
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'CreatedAt')
                BEGIN
                    ALTER TABLE [Messages] ADD [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE();
                END

                -- Ensure DeliveredAt column
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'DeliveredAt')
                BEGIN
                    ALTER TABLE [Messages] ADD [DeliveredAt] datetime2 NULL;
                END

                -- Ensure SeenAt column
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'SeenAt')
                BEGIN
                    ALTER TABLE [Messages] ADD [SeenAt] datetime2 NULL;
                END
                
                 -- Ensure ChatGroupId column
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'ChatGroupId')
                BEGIN
                    ALTER TABLE [Messages] ADD [ChatGroupId] int NULL;
                END
                
                -- Ensure EditedAt column
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'EditedAt')
                BEGIN
                    ALTER TABLE [Messages] ADD [EditedAt] datetime2 NULL;
                END
                
                -- Ensure IsEdited column
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'IsEdited')
                BEGIN
                    ALTER TABLE [Messages] ADD [IsEdited] bit NOT NULL DEFAULT 0;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
