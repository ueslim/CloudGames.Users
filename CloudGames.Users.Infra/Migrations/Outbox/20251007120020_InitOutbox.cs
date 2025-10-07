using System;
using CloudGames.Users.Infra.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CloudGames.Users.Infra.Migrations.Outbox;

[DbContext(typeof(OutboxContext))]
[Migration("20251007120020_InitOutbox")]
public partial class InitOutbox : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[OutboxMessages]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OutboxMessages](
        [Id] uniqueidentifier NOT NULL,
        [Type] nvarchar(128) NOT NULL,
        [Payload] nvarchar(max) NOT NULL,
        [OccurredAt] datetime2 NOT NULL,
        [ProcessedAt] datetime2 NULL,
        CONSTRAINT [PK_OutboxMessages] PRIMARY KEY ([Id])
    );
END
");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "OutboxMessages");
	}
}


