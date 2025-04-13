using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Techhive.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommendationAndTrendingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    R_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    P_Id = table.Column<int>(type: "int", nullable: false),
                    V_Count = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsOrdered = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.R_id);
                    table.ForeignKey(
                        name: "FK_Recommendations_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recommendations_Products_P_Id",
                        column: x => x.P_Id,
                        principalTable: "Products",
                        principalColumn: "P_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trendings",
                columns: table => new
                {
                    T_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    P_Id = table.Column<int>(type: "int", nullable: false),
                    TrendingScore = table.Column<double>(type: "float", nullable: false),
                    OrderCount = table.Column<int>(type: "int", nullable: false),
                    TotalViews = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trendings", x => x.T_id);
                    table.ForeignKey(
                        name: "FK_Trendings_Products_P_Id",
                        column: x => x.P_Id,
                        principalTable: "Products",
                        principalColumn: "P_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_P_Id",
                table: "Recommendations",
                column: "P_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_UserID",
                table: "Recommendations",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Trendings_P_Id",
                table: "Trendings",
                column: "P_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "Trendings");
        }
    }
}
