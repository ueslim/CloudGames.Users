using System;
using CloudGames.Users.Infra.EventStore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CloudGames.Users.Infra.Migrations.EventStore;

[DbContext(typeof(EventStoreSqlContext))]
[Migration("20251007120010_InitEventStore")]
public partial class InitEventStore : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[StoredEvents]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[StoredEvents](
        [Id] uniqueidentifier NOT NULL,
        [Type] nvarchar(128) NOT NULL,
        [Payload] nvarchar(max) NOT NULL,
        [OccurredAt] datetime2 NOT NULL,
        [Processed] bit NOT NULL CONSTRAINT [DF_StoredEvents_Processed] DEFAULT(0),
        CONSTRAINT [PK_StoredEvents] PRIMARY KEY ([Id])
    );
END
");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "StoredEvents");
	}
}


