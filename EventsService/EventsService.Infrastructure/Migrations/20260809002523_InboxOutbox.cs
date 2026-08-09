using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventsService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InboxOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_inbox_events_EventId",
                table: "inbox");

            migrationBuilder.DropIndex(
                name: "IX_inbox_EventId",
                table: "inbox");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "inbox");

            migrationBuilder.RenameColumn(
                name: "StatusComment",
                table: "inbox",
                newName: "Error");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProcessedAt",
                table: "inbox",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "inbox",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "inbox",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "outbox",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Topic = table.Column<string>(type: "text", nullable: false),
                    MessageKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    MessageType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "inbox");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "inbox");

            migrationBuilder.RenameColumn(
                name: "Error",
                table: "inbox",
                newName: "StatusComment");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ProcessedAt",
                table: "inbox",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "inbox",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_inbox_EventId",
                table: "inbox",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_inbox_events_EventId",
                table: "inbox",
                column: "EventId",
                principalTable: "events",
                principalColumn: "Id");
        }
    }
}
