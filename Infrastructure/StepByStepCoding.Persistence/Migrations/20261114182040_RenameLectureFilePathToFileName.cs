using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepByStepCoding.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameLectureFilePathToFileName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "LectureFiles",
                newName: "FileName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "LectureFiles",
                newName: "FilePath");
        }
    }
}
