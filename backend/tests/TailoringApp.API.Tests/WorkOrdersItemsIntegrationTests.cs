using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace TailoringApp.API.Tests;

public class WorkOrdersItemsIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public WorkOrdersItemsIntegrationTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Update_And_Remove_Item_Works()
    {
    var client = await _factory.CreateAuthenticatedClientAsync();

        var reg = await client.PostAsJsonAsync("/api/customers", new { CustomerNumber = "WO-IT", FirstName = "I", LastName = "T", Email = "it@test.com" });
        var cid = (await reg.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var wo = await client.PostAsJsonAsync($"/api/customers/{cid}/workorders", new { currency = "USD", appointmentId = (Guid?)null });
        var wid = (await wo.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        // Add initial item
        (await client.PostAsJsonAsync($"/api/customers/{cid}/workorders/{wid}/items", new { description = "Suit", quantity = 1, unitPrice = 200m, currency = "USD" })).StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Update quantity
        (await client.PutAsJsonAsync($"/api/customers/{cid}/workorders/{wid}/items/Suit", new { quantity = 2 })).StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify details show updated quantity and subtotal
        var details = await client.GetAsync($"/api/customers/{cid}/workorders/{wid}");
        details.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await details.Content.ReadFromJsonAsync<JsonElement>();
        dto.GetProperty("subtotal").GetDecimal().Should().Be(400m);
        dto.GetProperty("items")[0].GetProperty("quantity").GetInt32().Should().Be(2);

        // Remove the item
        (await client.DeleteAsync($"/api/customers/{cid}/workorders/{wid}/items/Suit")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify empty items and zero subtotal
        var details2 = await client.GetAsync($"/api/customers/{cid}/workorders/{wid}");
        details2.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto2 = await details2.Content.ReadFromJsonAsync<JsonElement>();
        dto2.GetProperty("subtotal").GetDecimal().Should().Be(0m);
        dto2.GetProperty("items").GetArrayLength().Should().Be(0);
    }
}
