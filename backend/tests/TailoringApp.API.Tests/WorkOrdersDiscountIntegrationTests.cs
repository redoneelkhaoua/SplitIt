using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace TailoringApp.API.Tests;

public class WorkOrdersDiscountIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public WorkOrdersDiscountIntegrationTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Set_And_Clear_Discount_Works()
    {
        var client = _factory.CreateClient();
        var reg = await client.PostAsJsonAsync("/api/customers", new { CustomerNumber = "WO-DISC", FirstName = "D", LastName = "S", Email = "disc@test.com" });
        var cid = (await reg.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var wo = await client.PostAsJsonAsync($"/api/customers/{cid}/workorders", new { currency = "USD", appointmentId = (Guid?)null });
        var wid = (await wo.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        // Add item
        (await client.PostAsJsonAsync($"/api/customers/{cid}/workorders/{wid}/items", new { description = "Pants", quantity = 2, unitPrice = 50m, currency = "USD" })).StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Set discount 30
        (await client.PostAsJsonAsync($"/api/customers/{cid}/workorders/{wid}/discount", new { amount = 30m, currency = "USD" })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var details = await client.GetAsync($"/api/customers/{cid}/workorders/{wid}");
        details.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await details.Content.ReadFromJsonAsync<JsonElement>();
        dto.GetProperty("subtotal").GetDecimal().Should().Be(100m);
        dto.GetProperty("discount").GetDecimal().Should().Be(30m);
        dto.GetProperty("total").GetDecimal().Should().Be(70m);

        // Clear discount
        (await client.DeleteAsync($"/api/customers/{cid}/workorders/{wid}/discount")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var details2 = await client.GetAsync($"/api/customers/{cid}/workorders/{wid}");
        var dto2 = await details2.Content.ReadFromJsonAsync<JsonElement>();
        dto2.GetProperty("discount").GetDecimal().Should().Be(0m);
        dto2.GetProperty("total").GetDecimal().Should().Be(100m);
    }
}
