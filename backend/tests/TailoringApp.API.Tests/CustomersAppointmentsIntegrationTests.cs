using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TailoringApp.Infrastructure.Persistence;
using Xunit;
using System.Text.Json;

namespace TailoringApp.API.Tests;

public class CustomersAppointmentsIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public CustomersAppointmentsIntegrationTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_Schedule_GetAppointments_Flow_Works()
    {
    var client = await _factory.CreateAuthenticatedClientAsync();

        var reg = new
        {
            CustomerNumber = "INT-001",
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = (DateTime?)null,
            Email = "int@test.com",
            Phone = (string?)null,
            Address = (string?)null,
            FitPreference = (string?)null,
            StylePreference = (string?)null,
            FabricPreference = (string?)null
        };

        var regResp = await client.PostAsJsonAsync("/api/customers", reg);
        regResp.StatusCode.Should().Be(HttpStatusCode.Created);
    var regElem = await regResp.Content.ReadFromJsonAsync<JsonElement>();
    Guid customerId = regElem.GetProperty("id").GetGuid();

        var start = DateTime.UtcNow.AddHours(1);
        var end = start.AddHours(1);
        var apptResp = await client.PostAsJsonAsync($"/api/customers/{customerId}/appointments", new { StartUtc = start, EndUtc = end, Notes = "int" });
        apptResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResp = await client.GetAsync($"/api/customers/{customerId}/appointments");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);

    // Complete the appointment
    var apptBody = await apptResp.Content.ReadFromJsonAsync<JsonElement>();
    var apptId = apptBody.GetProperty("id").GetGuid();
    var completeResp = await client.PostAsync($"/api/customers/{customerId}/appointments/{apptId}/complete", content: null);
    completeResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

    // Update notes
    var notesResp = await client.PatchAsJsonAsync($"/api/customers/{customerId}/appointments/{apptId}/notes", "updated note");
    notesResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

public class ApiFactory : WebApplicationFactory<Program>
{
    public async Task<HttpClient> CreateAuthenticatedClientAsync(string username = "admin", string password = "admin123")
    {
        var client = CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var token = json.GetProperty("token").GetString();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Replace SQL Server with InMemory for tests
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<TailoringDbContext>));
            services.Remove(descriptor);
            services.AddDbContext<TailoringDbContext>(opt => opt.UseInMemoryDatabase("tests-db"));
        });
        return base.CreateHost(builder);
    }
}
