using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVocabularyLevelAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Vocabularies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Vocabularies_TopicId_Word",
                table: "Vocabularies",
                columns: new[] { "TopicId", "Word" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TopicSources_TopicId_Textbook_Unit",
                table: "TopicSources",
                columns: new[] { "TopicId", "Textbook", "Unit" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vocabularies_TopicId_Word",
                table: "Vocabularies");

            migrationBuilder.DropIndex(
                name: "IX_TopicSources_TopicId_Textbook_Unit",
                table: "TopicSources");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Vocabularies");
        }
    }
}
