using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoGallery.Migrations
{
    /// <inheritdoc />
    public partial class ChageTableNameForImageLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Duplicates_Images_ImageId",
                table: "Duplicates");

            migrationBuilder.DropForeignKey(
                name: "FK_Duplicates_Labels_LabelId",
                table: "Duplicates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Duplicates",
                table: "Duplicates");

            migrationBuilder.RenameTable(
                name: "Duplicates",
                newName: "ImageLabels");

            migrationBuilder.RenameIndex(
                name: "IX_Duplicates_LabelId",
                table: "ImageLabels",
                newName: "IX_ImageLabels_LabelId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageLabels",
                table: "ImageLabels",
                columns: new[] { "ImageId", "LabelId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ImageLabels_Images_ImageId",
                table: "ImageLabels",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageLabels_Labels_LabelId",
                table: "ImageLabels",
                column: "LabelId",
                principalTable: "Labels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageLabels_Images_ImageId",
                table: "ImageLabels");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageLabels_Labels_LabelId",
                table: "ImageLabels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageLabels",
                table: "ImageLabels");

            migrationBuilder.RenameTable(
                name: "ImageLabels",
                newName: "Duplicates");

            migrationBuilder.RenameIndex(
                name: "IX_ImageLabels_LabelId",
                table: "Duplicates",
                newName: "IX_Duplicates_LabelId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Duplicates",
                table: "Duplicates",
                columns: new[] { "ImageId", "LabelId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Duplicates_Images_ImageId",
                table: "Duplicates",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Duplicates_Labels_LabelId",
                table: "Duplicates",
                column: "LabelId",
                principalTable: "Labels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
