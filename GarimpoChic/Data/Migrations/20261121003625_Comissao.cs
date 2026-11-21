using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarimpoChic.Data.Migrations
{
    /// <inheritdoc />
    public partial class Comissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PercentualVendedora",
                table: "PedidoItens",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VendedoraId",
                table: "PedidoItens",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Configuracoes",
                columns: table => new
                {
                    Chave = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Valor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracoes", x => x.Chave);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItens_VendedoraId",
                table: "PedidoItens",
                column: "VendedoraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuracoes");

            migrationBuilder.DropIndex(
                name: "IX_PedidoItens_VendedoraId",
                table: "PedidoItens");

            migrationBuilder.DropColumn(
                name: "PercentualVendedora",
                table: "PedidoItens");

            migrationBuilder.DropColumn(
                name: "VendedoraId",
                table: "PedidoItens");
        }
    }
}
