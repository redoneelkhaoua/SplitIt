using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace TailoringApp.API.Tests;

public class WorkOrdersDiscountNegativeIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public WorkOrdersDiscountNegativeIntegrationTests(ApiFactory factory) => _factory = factory;

    private async Task<(Guid customerId, Guid workOrderId)> CreateWorkOrder(HttpClient client)
    {
        var reg = await client.PostAsJsonAsync("/api/customers", new { CustomerNumber = Guid.NewGuid().ToString("N").Substring(0,8), FirstName = "Disc", LastName = "Test", Email = "disc.neg@test.com" });
        var cid = (await reg.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var wo = await client.PostAsJsonAsync($"/api/customers/{cid}/workorders", new { currency = "USD", appointmentId = (Guid?)null });
        var wid = (await wo.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        return (cid, wid);
    }

    [Fact]
    public async Task Setting_Discount_With_Mismatched_Currency_Fails()
    {
        var client = _factory.CreateClient();
        var (cid, wid) = await CreateWorkOrder(client);
        (await client.PostAsJsonAsync($"/api/customers/{cid}/workorders/{wid}/items", new { description = "Dress", quantity = 1, unitPrice = 80m, currency = "USD" })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var resp = await client.PostAsJsonAsync($"/api/customers/{cid}/workorders/{wid}/discount", new { amount = 10m, currency = "EUR" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Setting_Negative_Discount_Fails()
    {
        var client = _factory.CreateClient();
        var (cid, wid) = await CreateWorkOrder(client);
        (await client.PostAsJsonAsync($"/api/customers/{cid}/workorders/{wid}/items", new { description = "Shirt", quantity = 2, unitPrice = 40m, currency = "USD" })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var resp = await client.PostAsJsonAsync($"/api/customers/{cid}/workorders/{wid}/discount", new { amount = -5m, currency = "USD" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Setting_Discount_Larger_Than_Subtotal_Is_Capped()
    {
        var client = _factory.CreateClient();
        var (cid, wid) = await CreateWorkOrder(client);
        (await client.PostAsJsonAsync($"/api/customers/{cid}/workorders/{wid}/items", new { description = "Suit", quantity = 1, unitPrice = 120m, currency = "USD" })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync($"/api/customers/{cid}/workorders/{wid}/discount", new { amount = 1000m, currency = "USD" })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var details = await client.GetAsync($"/api/customers/{cid}/workorders/{wid}");
        var dto = await details.Content.ReadFromJsonAsync<JsonElement>();
        dto.GetProperty("subtotal").GetDecimal().Should().Be(120m);
        dto.GetProperty("discount").GetDecimal().Should().Be(120m); // capped
        dto.GetProperty("total").GetDecimal().Should().Be(0m);
    }

    [Fact]
    public async Task Cannot_Set_Discount_On_Completed_WorkOrder()
    {
        var client = _factory.CreateClient();
        var (cid, wid) = await CreateWorkOrder(client);
        (await client.PostAsJsonAsync($"/api/customers/{cid}/workorders/{wid}/items", new { description = "Vest", quantity = 1, unitPrice = 30m, currency = "USD" })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsync($"/api/customers/{cid}/workorders/{wid}/start", null!)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsync($"/api/customers/{cid}/workorders/{wid}/complete", null!)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var resp = await client.PostAsJsonAsync($"/api/customers/{cid}/workorders/{wid}/discount", new { amount = 5m, currency = "USD" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
