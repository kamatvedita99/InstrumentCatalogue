using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstrumentCatalogue.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInstrumentStatusHistoryUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_instrument_status_history_instrument_id_valid_from",
                table: "instrument_status_history");

            migrationBuilder.CreateIndex(
                name: "ix_instrument_status_history_instrument_id_effective_date_inst",
                table: "instrument_status_history",
                columns: new[] { "instrument_id", "effective_date", "instrument_status" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_instrument_status_history_instrument_id_effective_date_inst",
                table: "instrument_status_history");

            migrationBuilder.CreateIndex(
                name: "ix_instrument_status_history_instrument_id_valid_from",
                table: "instrument_status_history",
                columns: new[] { "instrument_id", "valid_from" },
                unique: true);
        }
    }
}
