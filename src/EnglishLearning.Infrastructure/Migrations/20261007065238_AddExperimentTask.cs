using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExperimentTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExperimentTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ExperimentSessionId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    Phase = table.Column<int>(type: "int", nullable: false),
                    TaskNumber = table.Column<int>(type: "int", nullable: false),
                    AttemptToken = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AdaptationApplied = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SubmitMoved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AllQuestionsAnsweredAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DurationMilliseconds = table.Column<long>(type: "bigint", nullable: true),
                    TimeToSubmitMilliseconds = table.Column<long>(type: "bigint", nullable: true),
                    OldSubmitLocationClickCount = table.Column<int>(type: "int", nullable: false),
                    IncompleteSubmitCount = table.Column<int>(type: "int", nullable: false),
                    QuizAttemptId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperimentTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExperimentTasks_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExperimentTasks_ExperimentSessions_ExperimentSessionId",
                        column: x => x.ExperimentSessionId,
                        principalTable: "ExperimentSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExperimentTasks_QuizAttempts_QuizAttemptId",
                        column: x => x.QuizAttemptId,
                        principalTable: "QuizAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentTasks_AttemptToken",
                table: "ExperimentTasks",
                column: "AttemptToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentTasks_ExerciseId",
                table: "ExperimentTasks",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentTasks_ExperimentSessionId_Phase_TaskNumber",
                table: "ExperimentTasks",
                columns: new[] { "ExperimentSessionId", "Phase", "TaskNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentTasks_QuizAttemptId",
                table: "ExperimentTasks",
                column: "QuizAttemptId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExperimentTasks");
        }
    }
}
