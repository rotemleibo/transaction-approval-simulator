using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionApproval.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeOutboxProcessorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_DeadLetteredAtUtc",
                table: "OutboxMessages");

            migrationBuilder.RenameColumn(
                name: "RetryCount",
                table: "OutboxMessages",
                newName: "Attempts");

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableAtUtc",
                table: "OutboxMessages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LeasedUntilUtc",
                table: "OutboxMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_DeadLetteredAtUtc_AvailableAtUtc_LeasedUntilUtc",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "DeadLetteredAtUtc", "AvailableAtUtc", "LeasedUntilUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_DeadLetteredAtUtc_AvailableAtUtc_LeasedUntilUtc",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "AvailableAtUtc",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "LeasedUntilUtc",
                table: "OutboxMessages");

            migrationBuilder.RenameColumn(
                name: "Attempts",
                table: "OutboxMessages",
                newName: "RetryCount");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_DeadLetteredAtUtc",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "DeadLetteredAtUtc" });
        }
    }
}
