using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstrumentCatalogue.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSymbolInstrumentPrimaryConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_symbol_x_ref_one_primary_per_instrument_symbol",
                table: "symbol_x_ref",
                column: "instrument_id",
                unique: true,
                filter: "is_primary = true AND valid_to = '9999-12-31'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_symbol_x_ref_one_primary_per_instrument_symbol",
                table: "symbol_x_ref");
        }
    }
}
