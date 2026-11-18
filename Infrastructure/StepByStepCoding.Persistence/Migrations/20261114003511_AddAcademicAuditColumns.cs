using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StepByStepCoding.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Submissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Submissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "Submissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Submissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Sections",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Sections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "Sections",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Sections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Lectures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Lectures",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "Lectures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Lectures",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "LectureFiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "LectureFiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "LectureFiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "LectureFiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Exams",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Exams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "Exams",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Exams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ExamQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "ExamQuestions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "ExamQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "ExamQuestions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ExamAttempts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "ExamAttempts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "ExamAttempts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "ExamAttempts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Enrollments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "Enrollments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Courses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Courses",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LectureFiles");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "LectureFiles");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "LectureFiles");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "LectureFiles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ExamQuestions");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "ExamQuestions");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "ExamQuestions");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "ExamQuestions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ExamAttempts");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "ExamAttempts");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "ExamAttempts");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "ExamAttempts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Courses");
        }
    }
}
