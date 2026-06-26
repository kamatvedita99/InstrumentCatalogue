using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstrumentCatalogue.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSymbolSymbologyUQConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_symbol_x_ref_symbology_id",
                table: "symbol_x_ref");

            migrationBuilder.CreateIndex(
                name: "ix_symbol_x_ref_symbology_id_symbol_current",
                table: "symbol_x_ref",
                columns: new[] { "symbology_id", "symbol" },
                unique: true,
                filter: "valid_to = '9999-12-31'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_symbol_x_ref_symbology_id_symbol_current",
                table: "symbol_x_ref");

            migrationBuilder.CreateIndex(
                name: "ix_symbol_x_ref_symbology_id",
                table: "symbol_x_ref",
                column: "symbology_id");
        }
    }
}
