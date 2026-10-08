using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExperimentSurvey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExperimentSurveys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ExperimentSessionId = table.Column<int>(type: "int", nullable: false),
                    PredictabilityRating = table.Column<int>(type: "int", nullable: false),
                    SubmitFindabilityRating = table.Column<int>(type: "int", nullable: false),
                    LayoutSatisfactionRating = table.Column<int>(type: "int", nullable: false),
                    OverallSatisfactionRating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperimentSurveys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExperimentSurveys_ExperimentSessions_ExperimentSessionId",
                        column: x => x.ExperimentSessionId,
                        principalTable: "ExperimentSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentSurveys_ExperimentSessionId",
                table: "ExperimentSurveys",
                column: "ExperimentSessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExperimentSurveys");
        }
    }
}
