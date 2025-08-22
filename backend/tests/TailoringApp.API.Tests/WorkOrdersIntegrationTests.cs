using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace TailoringApp.API.Tests;

public class WorkOrdersIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public WorkOrdersIntegrationTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Create_AddItem_Start_Complete_List_Works()
    {
        var client = _factory.CreateClient();

        // Register a customer
        var reg = new
        {
            CustomerNumber = $"WO-{Guid.NewGuid():N}".Substring(0, 10),
            FirstName = "WO",
            LastName = "User",
            DateOfBirth = (DateTime?)null,
            Email = $"wo{Guid.NewGuid():N}@test.com",
            Phone = (string?)null,
            Address = (string?)null,
            FitPreference = (string?)null,
            StylePreference = (string?)null,
            FabricPreference = (string?)null
        };
        var regResp = await client.PostAsJsonAsync("/api/customers", reg);
        regResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var regElem = await regResp.Content.ReadFromJsonAsync<JsonElement>();
        var customerId = regElem.GetProperty("id").GetGuid();

        // Create work order
        var createResp = await client.PostAsJsonAsync($"/api/customers/{customerId}/workorders", new { currency = "USD", appointmentId = (Guid?)null });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var createElem = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var workOrderId = createElem.GetProperty("id").GetGuid();

        // Add item
        var addItemResp = await client.PostAsJsonAsync($"/api/customers/{customerId}/workorders/{workOrderId}/items", new { description = "Jacket", quantity = 1, unitPrice = 100m, currency = "USD" });
        addItemResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Start
        var startResp = await client.PostAsync($"/api/customers/{customerId}/workorders/{workOrderId}/start", content: null);
        startResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Complete
        var completeResp = await client.PostAsync($"/api/customers/{customerId}/workorders/{workOrderId}/complete", content: null);
        completeResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // List
        var listResp = await client.GetAsync($"/api/customers/{customerId}/workorders?page=1&pageSize=10&sortBy=created&desc=true");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var listJson = await listResp.Content.ReadFromJsonAsync<JsonElement>();
        listJson.ValueKind.Should().Be(JsonValueKind.Array);
        listJson.GetArrayLength().Should().BeGreaterThan(0);

        var first = listJson.EnumerateArray().First();
        first.GetProperty("currency").GetString().Should().Be("USD");
        first.GetProperty("status").GetInt32().Should().Be(2); // Completed
        var items = first.GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);
        items[0].GetProperty("description").GetString().Should().Be("Jacket");
        items[0].GetProperty("unitPrice").GetDecimal().Should().Be(100m);
        first.GetProperty("subtotal").GetDecimal().Should().Be(100m);
    }
}
