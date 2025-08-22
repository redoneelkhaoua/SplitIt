using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace TailoringApp.API.Tests;

public class WorkOrdersNegativeIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public WorkOrdersNegativeIntegrationTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Create_WithForeignAppointment_ReturnsBadRequest()
    {
    var client = await _factory.CreateAuthenticatedClientAsync();
        // Create two customers
        var c1 = await client.PostAsJsonAsync("/api/customers", new { CustomerNumber = "WO-N1", FirstName = "A", LastName = "B", Email = "n1@test.com" });
        var c2 = await client.PostAsJsonAsync("/api/customers", new { CustomerNumber = "WO-N2", FirstName = "C", LastName = "D", Email = "n2@test.com" });
        var id1 = (await c1.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var id2 = (await c2.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        // Create appointment for customer 2
        var start = DateTime.UtcNow.AddHours(2);
        var end = start.AddHours(1);
        var appt = await client.PostAsJsonAsync($"/api/customers/{id2}/appointments", new { StartUtc = start, EndUtc = end, Notes = (string?)null });
        var apptId = (await appt.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        // Try to create work order for customer 1 linked to customer 2's appointment
        var create = await client.PostAsJsonAsync($"/api/customers/{id1}/workorders", new { currency = "USD", appointmentId = apptId });
        create.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddItem_CurrencyMismatch_ReturnsBadRequest()
    {
    var client = await _factory.CreateAuthenticatedClientAsync();
        var reg = await client.PostAsJsonAsync("/api/customers", new { CustomerNumber = "WO-N3", FirstName = "E", LastName = "F", Email = "n3@test.com" });
        var cid = (await reg.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var wo = await client.PostAsJsonAsync($"/api/customers/{cid}/workorders", new { currency = "USD", appointmentId = (Guid?)null });
        var wid = (await wo.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var add = await client.PostAsJsonAsync($"/api/customers/{cid}/workorders/{wid}/items", new { description = "Pants", quantity = 1, unitPrice = 50m, currency = "EUR" });
        add.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
