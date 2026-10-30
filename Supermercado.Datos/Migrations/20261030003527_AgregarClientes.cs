using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Supermercado.Datos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Rut = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    Nombres = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Apellidos = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    Telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Rut",
                table: "Clientes",
                column: "Rut",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
