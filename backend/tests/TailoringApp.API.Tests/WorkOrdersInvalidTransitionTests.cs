using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace TailoringApp.API.Tests;

public class WorkOrdersInvalidTransitionTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public WorkOrdersInvalidTransitionTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Start_WhenNotDraft_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var reg = await client.PostAsJsonAsync("/api/customers", new { CustomerNumber = "WO-TX", FirstName = "AA", LastName = "BB", Email = "tx@test.com" });
        var cid = (await reg.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var wo = await client.PostAsJsonAsync($"/api/customers/{cid}/workorders", new { currency = "USD", appointmentId = (Guid?)null });
        var wid = (await wo.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        // start -> ok
        (await client.PostAsync($"/api/customers/{cid}/workorders/{wid}/start", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        // start again -> bad request
        (await client.PostAsync($"/api/customers/{cid}/workorders/{wid}/start", null)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Complete_WhenNotInProgress_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var reg = await client.PostAsJsonAsync("/api/customers", new { CustomerNumber = "WO-TY", FirstName = "CC", LastName = "DD", Email = "ty@test.com" });
        var cid = (await reg.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var wo = await client.PostAsJsonAsync($"/api/customers/{cid}/workorders", new { currency = "USD", appointmentId = (Guid?)null });
        var wid = (await wo.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        // complete directly -> bad request
        (await client.PostAsync($"/api/customers/{cid}/workorders/{wid}/complete", null)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
