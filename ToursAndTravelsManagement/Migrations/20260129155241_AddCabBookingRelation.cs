using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ToursAndTravelsManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddCabBookingRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cabs",
                columns: table => new
                {
                    CabId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    HasAC = table.Column<bool>(type: "bit", nullable: false),
                    SeatingCapacity = table.Column<int>(type: "int", nullable: false),
                    BasePricePerKM = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Features = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DriverPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cabs", x => x.CabId);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    CityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PopularPlaces = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPopular = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.CityId);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    RouteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromCityId = table.Column<int>(type: "int", nullable: false),
                    ToCityId = table.Column<int>(type: "int", nullable: false),
                    Distance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedTime = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RouteDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PopularStops = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.RouteId);
                    table.ForeignKey(
                        name: "FK_Routes_Cities_FromCityId",
                        column: x => x.FromCityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Routes_Cities_ToCityId",
                        column: x => x.ToCityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CabRoutePrices",
                columns: table => new
                {
                    CabRoutePriceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CabId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    JourneyType = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncludedServices = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExcludedServices = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraCharges = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TermsAndConditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CabRoutePrices", x => x.CabRoutePriceId);
                    table.ForeignKey(
                        name: "FK_CabRoutePrices_Cabs_CabId",
                        column: x => x.CabId,
                        principalTable: "Cabs",
                        principalColumn: "CabId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CabRoutePrices_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "RouteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CabBookings",
                columns: table => new
                {
                    CabBookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GuestName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GuestEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GuestPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    CabRoutePriceId = table.Column<int>(type: "int", nullable: false),
                    TravelDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PickupTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    PickupLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DropoffLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NumberOfPassengers = table.Column<int>(type: "int", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExtraCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    PaymentTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedDriverId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedDriverName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedDriverPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CabId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CabBookings", x => x.CabBookingId);
                    table.ForeignKey(
                        name: "FK_CabBookings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CabBookings_CabRoutePrices_CabRoutePriceId",
                        column: x => x.CabRoutePriceId,
                        principalTable: "CabRoutePrices",
                        principalColumn: "CabRoutePriceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CabBookings_Cabs_CabId",
                        column: x => x.CabId,
                        principalTable: "Cabs",
                        principalColumn: "CabId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CabBookingInvoices",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CabBookingId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BillingAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BaseFare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TollCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DriverAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ParkingCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NightCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GST = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceDue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CabBookingInvoices", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK_CabBookingInvoices_CabBookings_CabBookingId",
                        column: x => x.CabBookingId,
                        principalTable: "CabBookings",
                        principalColumn: "CabBookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cabs",
                columns: new[] { "CabId", "BasePricePerKM", "CreatedBy", "CreatedDate", "DriverId", "DriverName", "DriverPhone", "Features", "HasAC", "ImageUrl", "IsActive", "Model", "Name", "RegistrationNumber", "SeatingCapacity", "Status", "Type", "Year" },
                values: new object[,]
                {
                    { 1, 15m, null, new DateTime(2026, 1, 29, 21, 22, 38, 808, DateTimeKind.Local).AddTicks(1197), null, null, null, "[\"Music System\", \"Airbags\", \"Power Windows\"]", true, null, true, "2023", "SWIFT DZIRE", "MH12AB1234", 4, 1, 2, 2023 },
                    { 2, 25m, null, new DateTime(2026, 1, 29, 21, 22, 38, 808, DateTimeKind.Local).AddTicks(1204), null, null, null, "[\"Rear AC\", \"Leather Seats\", \"Entertainment System\"]", true, null, true, "2022", "TOYOTA INNOVA", "MH12CD5678", 7, 1, 4, 2022 }
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "CityId", "Country", "CreatedBy", "CreatedDate", "Description", "ImageUrl", "IsActive", "IsPopular", "Name", "PopularPlaces", "State" },
                values: new object[,]
                {
                    { 1, "India", null, new DateTime(2026, 1, 29, 21, 22, 38, 808, DateTimeKind.Local).AddTicks(294), null, null, true, true, "Pune", null, "Maharashtra" },
                    { 2, "India", null, new DateTime(2026, 1, 29, 21, 22, 38, 808, DateTimeKind.Local).AddTicks(308), null, null, true, true, "Mumbai", null, "Maharashtra" },
                    { 3, "India", null, new DateTime(2026, 1, 29, 21, 22, 38, 808, DateTimeKind.Local).AddTicks(310), null, null, true, true, "Goa", null, "Goa" },
                    { 4, "India", null, new DateTime(2026, 1, 29, 21, 22, 38, 808, DateTimeKind.Local).AddTicks(311), null, null, true, true, "Nagpur", null, "Maharashtra" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CabBookingInvoices_CabBookingId",
                table: "CabBookingInvoices",
                column: "CabBookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CabBookings_BookingNumber",
                table: "CabBookings",
                column: "BookingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CabBookings_CabId",
                table: "CabBookings",
                column: "CabId");

            migrationBuilder.CreateIndex(
                name: "IX_CabBookings_CabRoutePriceId",
                table: "CabBookings",
                column: "CabRoutePriceId");

            migrationBuilder.CreateIndex(
                name: "IX_CabBookings_UserId",
                table: "CabBookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CabRoutePrices_CabId_RouteId_JourneyType",
                table: "CabRoutePrices",
                columns: new[] { "CabId", "RouteId", "JourneyType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CabRoutePrices_RouteId",
                table: "CabRoutePrices",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Cabs_RegistrationNumber",
                table: "Cabs",
                column: "RegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name_State",
                table: "Cities",
                columns: new[] { "Name", "State" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_FromCityId_ToCityId",
                table: "Routes",
                columns: new[] { "FromCityId", "ToCityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_ToCityId",
                table: "Routes",
                column: "ToCityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CabBookingInvoices");

            migrationBuilder.DropTable(
                name: "CabBookings");

            migrationBuilder.DropTable(
                name: "CabRoutePrices");

            migrationBuilder.DropTable(
                name: "Cabs");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
