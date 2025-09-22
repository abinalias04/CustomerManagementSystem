using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Entity.Migrations
{
    /// <inheritdoc />
    public partial class cartsoftdel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Carts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Carts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CartItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CartItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            //migrationBuilder.CreateTable(
            //    name: "States",
            //    columns: table => new
            //    {
            //        StateId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        StateName = table.Column<string>(type: "nvarchar(450)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_States", x => x.StateId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Districts",
            //    columns: table => new
            //    {
            //        DistrictId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DistrictName = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        StateId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Districts", x => x.DistrictId);
            //        table.ForeignKey(
            //            name: "FK_Districts_States_StateId",
            //            column: x => x.StateId,
            //            principalTable: "States",
            //            principalColumn: "StateId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Pincodes",
            //    columns: table => new
            //    {
            //        PincodeId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Pincode = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        DistrictId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Pincodes", x => x.PincodeId);
            //        table.ForeignKey(
            //            name: "FK_Pincodes_Districts_DistrictId",
            //            column: x => x.DistrictId,
            //            principalTable: "Districts",
            //            principalColumn: "DistrictId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "PostOffices",
            //    columns: table => new
            //    {
            //        PostOfficeId = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        PostOfficeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PincodeId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PostOffices", x => x.PostOfficeId);
            //        table.ForeignKey(
            //            name: "FK_PostOffices_Pincodes_PincodeId",
            //            column: x => x.PincodeId,
            //            principalTable: "Pincodes",
            //            principalColumn: "PincodeId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUsers_DistrictId",
            //    table: "AspNetUsers",
            //    column: "DistrictId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUsers_PincodeId",
            //    table: "AspNetUsers",
            //    column: "PincodeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUsers_PostOfficeId",
            //    table: "AspNetUsers",
            //    column: "PostOfficeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUsers_StateId",
            //    table: "AspNetUsers",
            //    column: "StateId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Districts_StateId_DistrictName",
            //    table: "Districts",
            //    columns: new[] { "StateId", "DistrictName" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Pincodes_DistrictId_Pincode",
            //    table: "Pincodes",
            //    columns: new[] { "DistrictId", "Pincode" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_PostOffices_PincodeId",
            //    table: "PostOffices",
            //    column: "PincodeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_States_StateName",
            //    table: "States",
            //    column: "StateName",
            //    unique: true);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AspNetUsers_Districts_DistrictId",
            //    table: "AspNetUsers",
            //    column: "DistrictId",
            //    principalTable: "Districts",
            //    principalColumn: "DistrictId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AspNetUsers_Pincodes_PincodeId",
            //    table: "AspNetUsers",
            //    column: "PincodeId",
            //    principalTable: "Pincodes",
            //    principalColumn: "PincodeId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AspNetUsers_PostOffices_PostOfficeId",
            //    table: "AspNetUsers",
            //    column: "PostOfficeId",
            //    principalTable: "PostOffices",
            //    principalColumn: "PostOfficeId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AspNetUsers_States_StateId",
            //    table: "AspNetUsers",
            //    column: "StateId",
            //    principalTable: "States",
            //    principalColumn: "StateId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "CartId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_AspNetUsers_Districts_DistrictId",
            //    table: "AspNetUsers");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_AspNetUsers_Pincodes_PincodeId",
            //    table: "AspNetUsers");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_AspNetUsers_PostOffices_PostOfficeId",
            //    table: "AspNetUsers");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_AspNetUsers_States_StateId",
            //    table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems");

            //migrationBuilder.DropTable(
            //    name: "PostOffices");

            //migrationBuilder.DropTable(
            //    name: "Pincodes");

            //migrationBuilder.DropTable(
            //    name: "Districts");

            //migrationBuilder.DropTable(
            //    name: "States");

            //migrationBuilder.DropIndex(
            //    name: "IX_AspNetUsers_DistrictId",
            //    table: "AspNetUsers");

            //migrationBuilder.DropIndex(
            //    name: "IX_AspNetUsers_PincodeId",
            //    table: "AspNetUsers");

            //migrationBuilder.DropIndex(
            //    name: "IX_AspNetUsers_PostOfficeId",
            //    table: "AspNetUsers");

            //migrationBuilder.DropIndex(
            //    name: "IX_AspNetUsers_StateId",
            //    table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CartItems");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "CartId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
