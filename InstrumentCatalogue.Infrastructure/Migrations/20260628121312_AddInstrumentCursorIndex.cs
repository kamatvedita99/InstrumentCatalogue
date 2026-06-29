using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstrumentCatalogue.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInstrumentCursorIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_instruments_created_at_utc_instrument_id",
                table: "instruments",
                columns: new[] { "created_at_utc", "instrument_id" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_instruments_created_at_utc_instrument_id",
                table: "instruments");
        }
    }
}
