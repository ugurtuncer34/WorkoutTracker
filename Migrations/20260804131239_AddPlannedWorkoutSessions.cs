using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddPlannedWorkoutSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TemplateCategorySnapshot",
                table: "WorkoutSessions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateNameSnapshot",
                table: "WorkoutSessions",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkoutTemplateId",
                table: "WorkoutSessions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkoutSessionExerciseId",
                table: "SetLogs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkoutSessionExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkoutSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExerciseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAdHoc = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlannedSetCount = table.Column<int>(type: "INTEGER", nullable: false),
                    RepMin = table.Column<int>(type: "INTEGER", nullable: true),
                    RepMax = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetDurationSeconds = table.Column<int>(type: "INTEGER", nullable: true),
                    SuggestedWeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NotesSnapshot = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsOptional = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExerciseNameSnapshot = table.Column<string>(type: "TEXT", nullable: false),
                    ExerciseTypeSnapshot = table.Column<int>(type: "INTEGER", nullable: false),
                    IconKeySnapshot = table.Column<string>(type: "TEXT", nullable: true),
                    TargetMuscleNameSnapshot = table.Column<string>(type: "TEXT", nullable: false),
                    MuscleGroupNameSnapshot = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessionExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutSessionExercises_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutSessionExercises_WorkoutSessions_WorkoutSessionId",
                        column: x => x.WorkoutSessionId,
                        principalTable: "WorkoutSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_WorkoutTemplateId",
                table: "WorkoutSessions",
                column: "WorkoutTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SetLogs_WorkoutSessionExerciseId",
                table: "SetLogs",
                column: "WorkoutSessionExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionExercises_ExerciseId",
                table: "WorkoutSessionExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionExercises_WorkoutSessionId",
                table: "WorkoutSessionExercises",
                column: "WorkoutSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionExercises_WorkoutSessionId_ExerciseId",
                table: "WorkoutSessionExercises",
                columns: new[] { "WorkoutSessionId", "ExerciseId" },
                unique: true,
                filter: "IsAdHoc = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionExercises_WorkoutSessionId_Position",
                table: "WorkoutSessionExercises",
                columns: new[] { "WorkoutSessionId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessionExercises_WorkoutSessionId_Status",
                table: "WorkoutSessionExercises",
                columns: new[] { "WorkoutSessionId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_SetLogs_WorkoutSessionExercises_WorkoutSessionExerciseId",
                table: "SetLogs",
                column: "WorkoutSessionExerciseId",
                principalTable: "WorkoutSessionExercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutSessions_WorkoutTemplates_WorkoutTemplateId",
                table: "WorkoutSessions",
                column: "WorkoutTemplateId",
                principalTable: "WorkoutTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SetLogs_WorkoutSessionExercises_WorkoutSessionExerciseId",
                table: "SetLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutSessions_WorkoutTemplates_WorkoutTemplateId",
                table: "WorkoutSessions");

            migrationBuilder.DropTable(
                name: "WorkoutSessionExercises");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutSessions_WorkoutTemplateId",
                table: "WorkoutSessions");

            migrationBuilder.DropIndex(
                name: "IX_SetLogs_WorkoutSessionExerciseId",
                table: "SetLogs");

            migrationBuilder.DropColumn(
                name: "TemplateCategorySnapshot",
                table: "WorkoutSessions");

            migrationBuilder.DropColumn(
                name: "TemplateNameSnapshot",
                table: "WorkoutSessions");

            migrationBuilder.DropColumn(
                name: "WorkoutTemplateId",
                table: "WorkoutSessions");

            migrationBuilder.DropColumn(
                name: "WorkoutSessionExerciseId",
                table: "SetLogs");
        }
    }
}
