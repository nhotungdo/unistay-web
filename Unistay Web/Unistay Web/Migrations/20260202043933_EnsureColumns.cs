using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unistay_Web.Migrations
{
    /// <inheritdoc />
    public partial class EnsureColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'ChatGroupId')
                BEGIN
                    ALTER TABLE [Messages] ADD [ChatGroupId] int NULL;
                END
                
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'EditedAt')
                BEGIN
                    ALTER TABLE [Messages] ADD [EditedAt] datetime2 NULL;
                END
                
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'IsEdited')
                BEGIN
                    ALTER TABLE [Messages] ADD [IsEdited] bit NOT NULL DEFAULT 0;
                END
            ");
            
            // Check constraints separately
            migrationBuilder.Sql(@"
               IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Messages' AND COLUMN_NAME = 'ChatGroupId')
               BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Messages_ChatGroups_ChatGroupId')
                    BEGIN
                        -- Ensure ChatGroups exists before adding FK
                        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ChatGroups')
                        BEGIN
                             ALTER TABLE [Messages] ADD CONSTRAINT [FK_Messages_ChatGroups_ChatGroupId] FOREIGN KEY ([ChatGroupId]) REFERENCES [ChatGroups] ([Id]);
                        END
                    END
                    
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Messages_ChatGroupId')
                    BEGIN
                        CREATE INDEX [IX_Messages_ChatGroupId] ON [Messages] ([ChatGroupId]);
                    END
               END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
