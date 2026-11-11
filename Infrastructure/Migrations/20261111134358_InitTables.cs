using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Ayan");

            migrationBuilder.CreateTable(
                name: "InquiryRequests",
                schema: "Ayan",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RequestHash = table.Column<string>(type: "char(64)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    BypassCache = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", unicode: false, maxLength: 24, nullable: false),
                    ResultOutcome = table.Column<byte>(type: "tinyint", unicode: false, maxLength: 24, nullable: true),
                    ReasonCode = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    ResultProviderKey = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    ResultSource = table.Column<byte>(type: "tinyint", unicode: false, maxLength: 12, nullable: true),
                    ResultObservedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessingDeadlineUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InquiryRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InquiryProviderAttempts",
                schema: "Ayan",
                columns: table => new
                {
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    InquiryRequestId = table.Column<long>(type: "bigint", nullable: false),
                    ProviderKey = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    Outcome = table.Column<byte>(type: "tinyint", unicode: false, maxLength: 24, nullable: false),
                    ErrorCode = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FinishedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InquiryProviderAttempts", x => new { x.InquiryRequestId, x.AttemptNumber });
                    table.ForeignKey(
                        name: "FK_InquiryProviderAttempts_InquiryRequests_InquiryRequestId",
                        column: x => x.InquiryRequestId,
                        principalSchema: "Ayan",
                        principalTable: "InquiryRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InquiryRequests_IdempotencyKey",
                schema: "Ayan",
                table: "InquiryRequests",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InquiryProviderAttempts",
                schema: "Ayan");

            migrationBuilder.DropTable(
                name: "InquiryRequests",
                schema: "Ayan");
        }
    }
}
