using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InstrumentCatalogue.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "instruments",
                columns: table => new
                {
                    instrument_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    exchange = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    currency = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    country = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    listed_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_instruments", x => x.instrument_id);
                });

            migrationBuilder.CreateTable(
                name: "symbologies",
                columns: table => new
                {
                    symbology_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_symbologies", x => x.symbology_id);
                });

            migrationBuilder.CreateTable(
                name: "vendors",
                columns: table => new
                {
                    vendor_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    short_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendors", x => x.vendor_id);
                });

            migrationBuilder.CreateTable(
                name: "bond_ref_data",
                columns: table => new
                {
                    instrument_id = table.Column<Guid>(type: "uuid", nullable: false),
                    face_value = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    coupon_rate = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: true),
                    coupon_frequency = table.Column<string>(type: "text", nullable: true),
                    issuer = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    issue_date = table.Column<DateOnly>(type: "date", nullable: true),
                    maturity_date = table.Column<DateOnly>(type: "date", nullable: true),
                    credit_rating = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    bond_type = table.Column<string>(type: "text", nullable: true),
                    bond_structure = table.Column<string>(type: "text", nullable: true),
                    duration = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bond_ref_data", x => x.instrument_id);
                    table.ForeignKey(
                        name: "fk_bond_ref_data_instruments_instrument_id",
                        column: x => x.instrument_id,
                        principalTable: "instruments",
                        principalColumn: "instrument_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equity_ref_data",
                columns: table => new
                {
                    instrument_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sector = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    industry = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    shares_outstanding = table.Column<long>(type: "bigint", nullable: true),
                    lot_size = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    par_value = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equity_ref_data", x => x.instrument_id);
                    table.ForeignKey(
                        name: "fk_equity_ref_data_instruments_instrument_id",
                        column: x => x.instrument_id,
                        principalTable: "instruments",
                        principalColumn: "instrument_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "etf_ref_data",
                columns: table => new
                {
                    instrument_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fund_manager = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    underlying_index = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    replication_type = table.Column<string>(type: "text", nullable: false),
                    distribution_frequency = table.Column<string>(type: "text", nullable: true),
                    inception_date = table.Column<DateOnly>(type: "date", nullable: true),
                    expense_ratio = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_etf_ref_data", x => x.instrument_id);
                    table.ForeignKey(
                        name: "fk_etf_ref_data_instruments_instrument_id",
                        column: x => x.instrument_id,
                        principalTable: "instruments",
                        principalColumn: "instrument_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "instrument_status_history",
                columns: table => new
                {
                    instrument_status_history_id = table.Column<Guid>(type: "uuid", nullable: false),
                    instrument_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "'9999-12-31'::date"),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    instrument_status = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_instrument_status_history", x => x.instrument_status_history_id);
                    table.ForeignKey(
                        name: "fk_instrument_status_history_instruments_instrument_id",
                        column: x => x.instrument_id,
                        principalTable: "instruments",
                        principalColumn: "instrument_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "symbol_x_ref",
                columns: table => new
                {
                    symbol_x_ref_id = table.Column<Guid>(type: "uuid", nullable: false),
                    symbology_id = table.Column<int>(type: "integer", nullable: false),
                    symbol = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    instrument_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: true),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "'9999-12-31'::date"),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_symbol_x_ref", x => x.symbol_x_ref_id);
                    table.ForeignKey(
                        name: "fk_symbol_x_ref_instruments_instrument_id",
                        column: x => x.instrument_id,
                        principalTable: "instruments",
                        principalColumn: "instrument_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_symbol_x_ref_symbologies_symbology_id",
                        column: x => x.symbology_id,
                        principalTable: "symbologies",
                        principalColumn: "symbology_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vendor_interfaces",
                columns: table => new
                {
                    vendor_interface_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vendor_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    protocol = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendor_interfaces", x => x.vendor_interface_id);
                    table.ForeignKey(
                        name: "fk_vendor_interfaces_vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "vendors",
                        principalColumn: "vendor_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vendor_interface_symbol_x_ref",
                columns: table => new
                {
                    vendor_interface_symbol_x_ref_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_interface_id = table.Column<int>(type: "integer", nullable: false),
                    symbol_x_ref_id = table.Column<Guid>(type: "uuid", nullable: false),
                    received_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendor_interface_symbol_x_ref", x => x.vendor_interface_symbol_x_ref_id);
                    table.ForeignKey(
                        name: "fk_vendor_interface_symbol_x_ref_symbol_x_ref_symbol_x_ref_id",
                        column: x => x.symbol_x_ref_id,
                        principalTable: "symbol_x_ref",
                        principalColumn: "symbol_x_ref_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vendor_interface_symbol_x_ref_vendor_interfaces_vendor_inte",
                        column: x => x.vendor_interface_id,
                        principalTable: "vendor_interfaces",
                        principalColumn: "vendor_interface_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_status_current",
                table: "instrument_status_history",
                column: "instrument_id",
                filter: "valid_to = '9999-12-31'");

            migrationBuilder.CreateIndex(
                name: "ix_instrument_status_history_instrument_id_valid_from",
                table: "instrument_status_history",
                columns: new[] { "instrument_id", "valid_from" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_instruments_type",
                table: "instruments",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "idx_symbol_x_ref_current",
                table: "symbol_x_ref",
                column: "symbol",
                filter: "valid_to = '9999-12-31'");

            migrationBuilder.CreateIndex(
                name: "ix_symbol_x_ref_instrument_id_symbology_id_valid_from",
                table: "symbol_x_ref",
                columns: new[] { "instrument_id", "symbology_id", "valid_from" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_symbol_x_ref_symbology_id",
                table: "symbol_x_ref",
                column: "symbology_id");

            migrationBuilder.CreateIndex(
                name: "ix_symbologies_type_code",
                table: "symbologies",
                column: "type_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vendor_interface_symbol_x_ref_symbol_x_ref_id",
                table: "vendor_interface_symbol_x_ref",
                column: "symbol_x_ref_id");

            migrationBuilder.CreateIndex(
                name: "ix_vendor_interface_symbol_x_ref_vendor_interface_id_symbol_x_",
                table: "vendor_interface_symbol_x_ref",
                columns: new[] { "vendor_interface_id", "symbol_x_ref_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vendor_interfaces_vendor_id_name",
                table: "vendor_interfaces",
                columns: new[] { "vendor_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vendors_name",
                table: "vendors",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vendors_short_code",
                table: "vendors",
                column: "short_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bond_ref_data");

            migrationBuilder.DropTable(
                name: "equity_ref_data");

            migrationBuilder.DropTable(
                name: "etf_ref_data");

            migrationBuilder.DropTable(
                name: "instrument_status_history");

            migrationBuilder.DropTable(
                name: "vendor_interface_symbol_x_ref");

            migrationBuilder.DropTable(
                name: "symbol_x_ref");

            migrationBuilder.DropTable(
                name: "vendor_interfaces");

            migrationBuilder.DropTable(
                name: "instruments");

            migrationBuilder.DropTable(
                name: "symbologies");

            migrationBuilder.DropTable(
                name: "vendors");
        }
    }
}
