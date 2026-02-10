using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvancedDevSample.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string AuditLogsTable = "AuditLogs";
            const string UsersTable = "Users";
            const string ProductsTable = "Products";
            const string RefreshTokensTable = "RefreshTokens";
            const string PriceHistoriesTable = "PriceHistories";
            const string BooleanType = "boolean";
            const string TimestampWithTimeZoneType = "timestamp with time zone";

            migrationBuilder.CreateTable(
                name: AuditLogsTable,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsSuccess = table.Column<bool>(type: BooleanType, nullable: false),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: TimestampWithTimeZoneType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: BooleanType, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: TimestampWithTimeZoneType, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: TimestampWithTimeZoneType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: UsersTable,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: BooleanType, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: TimestampWithTimeZoneType, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: TimestampWithTimeZoneType, nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: TimestampWithTimeZoneType, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: ProductsTable,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: BooleanType, nullable: false),
                    Sku = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiscountPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: TimestampWithTimeZoneType, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: TimestampWithTimeZoneType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: RefreshTokensTable,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: TimestampWithTimeZoneType, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: TimestampWithTimeZoneType, nullable: false),
                    IsRevoked = table.Column<bool>(type: BooleanType, nullable: false),
                    RevokedAt = table.Column<DateTime>(type: TimestampWithTimeZoneType, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: UsersTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: PriceHistoriesTable,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NewPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: TimestampWithTimeZoneType, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceHistories_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: ProductsTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: AuditLogsTable,
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EventType",
                table: AuditLogsTable,
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: AuditLogsTable,
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_ChangedAt",
                table: PriceHistoriesTable,
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_ProductId",
                table: PriceHistoriesTable,
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: ProductsTable,
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                table: ProductsTable,
                column: "Sku",
                unique: true,
                filter: "\"Sku\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: RefreshTokensTable,
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: RefreshTokensTable,
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: UsersTable,
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            const string AuditLogsTable = "AuditLogs";
            const string UsersTable = "Users";
            const string ProductsTable = "Products";
            const string RefreshTokensTable = "RefreshTokens";
            const string PriceHistoriesTable = "PriceHistories";

            migrationBuilder.DropTable(
                name: AuditLogsTable);

            migrationBuilder.DropTable(
                name: PriceHistoriesTable);

            migrationBuilder.DropTable(
                name: RefreshTokensTable);

            migrationBuilder.DropTable(
                name: ProductsTable);

            migrationBuilder.DropTable(
                name: UsersTable);

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
