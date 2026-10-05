using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Carpool.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Quiet" },
                    { 2, "Music" },
                    { 3, "PetsAllowed" },
                    { 4, "SmokingForbidden" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "PasswordHash", "PhoneNumber", "Role" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@carpool.dev", "Admin", true, "User", "AQAAAAIAAYagAAAAEBUbLMKM4BpncNZwJpDpoOdp7Iyxiec5eZYskgkZMkOCzUATnykIFmg52+g+igRoUQ==", "+972500000001", "Admin" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "PasswordHash", "PhoneNumber" },
                values: new object[,]
                {
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "alice@carpool.dev", "Alice", true, "Cohen", "AQAAAAIAAYagAAAAEBUbLMKM4BpncNZwJpDpoOdp7Iyxiec5eZYskgkZMkOCzUATnykIFmg52+g+igRoUQ==", "+972500000002" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bob@carpool.dev", "Bob", true, "Levi", "AQAAAAIAAYagAAAAEBUbLMKM4BpncNZwJpDpoOdp7Iyxiec5eZYskgkZMkOCzUATnykIFmg52+g+igRoUQ==", "+972500000003" },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "carol@carpool.dev", "Carol", true, "Mizrahi", "AQAAAAIAAYagAAAAEBUbLMKM4BpncNZwJpDpoOdp7Iyxiec5eZYskgkZMkOCzUATnykIFmg52+g+igRoUQ==", "+972500000004" }
                });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "LicensePlate", "Manufacturer", "Model", "OwnerId", "PassengerCapacity" },
                values: new object[,]
                {
                    { 1, "111-11-111", "Toyota", "Corolla", 2, 4 },
                    { 2, "222-22-222", "Honda", "Civic", 3, 3 },
                    { 3, "333-33-333", "Mazda", "3", 4, 5 }
                });

            migrationBuilder.InsertData(
                table: "Rides",
                columns: new[] { "Id", "AvailableSeats", "CompletedAt", "CreatedAt", "DepartureTime", "Destination", "DriverId", "EstimatedDurationMinutes", "Origin", "PricePerSeat", "StartedAt", "TotalSeats", "VehicleId" },
                values: new object[,]
                {
                    { 1, 3, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2030, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Jerusalem", 2, 60, "Tel Aviv", 25.00m, null, 3, 1 },
                    { 2, 2, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2030, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), "Tel Aviv", 3, 90, "Haifa", 30.00m, null, 2, 2 }
                });

            migrationBuilder.InsertData(
                table: "RideTags",
                columns: new[] { "RideId", "TagId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 4 },
                    { 2, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RideTags",
                keyColumns: new[] { "RideId", "TagId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "RideTags",
                keyColumns: new[] { "RideId", "TagId" },
                keyValues: new object[] { 1, 4 });

            migrationBuilder.DeleteData(
                table: "RideTags",
                keyColumns: new[] { "RideId", "TagId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Rides",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Rides",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
