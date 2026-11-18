using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepByStepCoding.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LectureFiles_LectureId",
                table: "LectureFiles");

            migrationBuilder.CreateTable(
                name: "WorkspaceFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Language = table.Column<int>(type: "int", nullable: false),
                    SavedContent = table.Column<string>(type: "nvarchar(max)", maxLength: 100000, nullable: false),
                    SavedRevision = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LectureFiles_LectureId",
                table: "LectureFiles",
                column: "LectureId",
                unique: true,
                filter: "[LectureId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceFiles_UserId",
                table: "WorkspaceFiles",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkspaceFiles");

            migrationBuilder.DropIndex(
                name: "IX_LectureFiles_LectureId",
                table: "LectureFiles");

            migrationBuilder.CreateIndex(
                name: "IX_LectureFiles_LectureId",
                table: "LectureFiles",
                column: "LectureId",
                unique: true);
        }
    }
}
