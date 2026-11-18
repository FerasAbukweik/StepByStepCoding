using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepByStepCoding.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OneFilePerLecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LectureFiles_LectureId_FilePath",
                table: "LectureFiles");

            migrationBuilder.CreateIndex(
                name: "IX_LectureFiles_LectureId",
                table: "LectureFiles",
                column: "LectureId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LectureFiles_LectureId",
                table: "LectureFiles");

            migrationBuilder.CreateIndex(
                name: "IX_LectureFiles_LectureId_FilePath",
                table: "LectureFiles",
                columns: new[] { "LectureId", "FilePath" },
                unique: true);
        }
    }
}
