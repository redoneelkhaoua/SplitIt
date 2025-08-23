using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TailoringApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGarmentTypeAndMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ChestMeasurement",
                table: "WorkOrderItems",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GarmentType",
                table: "WorkOrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "HipsMeasurement",
                table: "WorkOrderItems",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeasurementNotes",
                table: "WorkOrderItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SleeveMeasurement",
                table: "WorkOrderItems",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WaistMeasurement",
                table: "WorkOrderItems",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChestMeasurement",
                table: "WorkOrderItems");

            migrationBuilder.DropColumn(
                name: "GarmentType",
                table: "WorkOrderItems");

            migrationBuilder.DropColumn(
                name: "HipsMeasurement",
                table: "WorkOrderItems");

            migrationBuilder.DropColumn(
                name: "MeasurementNotes",
                table: "WorkOrderItems");

            migrationBuilder.DropColumn(
                name: "SleeveMeasurement",
                table: "WorkOrderItems");

            migrationBuilder.DropColumn(
                name: "WaistMeasurement",
                table: "WorkOrderItems");
        }
    }
}
