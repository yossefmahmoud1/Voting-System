using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VotingSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedIdentityRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "IsDefault", "IsDeleted", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760", "c43ec441-b61b-4696-b00f-444a36cdfa51", false, false, "Admin", "ADMIN" },
                    { "b4b3aa47-ed63-4777-b73a-c6124836997a", "e4441325-3953-4ee2-9b95-dd67bf32922e", true, false, "Member", "MEMBER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FristName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "58a9eb9b-f8b9-4267-a7d7-006b016d6ba0", 0, "fffe5055-7d79-4ad7-a07f-74a10069aea9", "admin@surveySystem.com", true, "", "", false, null, "ADMIN@SURVEYSYSTEM.COM", "ADMIN1", "AQAAAAIAAYagAAAAEHHle97gKTePm1/xXB7+FecjiCvo9JyrCl7biGp/YBzNhWlHD4JZKzoWhBODgusZDg==", null, false, "7d97f47978b740f822f2ac6d1f3aac6", false, "admin1" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 1, "permissions", "polls.read", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" },
                    { 2, "permissions", "polls.add", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" },
                    { 3, "permissions", "polls.update", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" },
                    { 4, "permissions", "polls.delete", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" },
                    { 5, "permissions", "questions.read", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" },
                    { 6, "permissions", "questions.add", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" },
                    { 7, "permissions", "questions.update", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" },
                    { 8, "permissions", "users.read", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" },
                    { 9, "permissions", "users.add", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" },
                    { 10, "permissions", "users.update", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" },
                    { 11, "permissions", "roles.read", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" },
                    { 12, "permissions", "roles.add", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" },
                    { 13, "permissions", "roles.update", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" },
                    { 14, "permissions", "results.read", "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760", "58a9eb9b-f8b9-4267-a7d7-006b016d6ba0" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b4b3aa47-ed63-4777-b73a-c6124836997a");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760", "58a9eb9b-f8b9-4267-a7d7-006b016d6ba0" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5cdc5d78-ecdc-4f8f-805f-eb3f42f15760");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "58a9eb9b-f8b9-4267-a7d7-006b016d6ba0");
        }
    }
}
