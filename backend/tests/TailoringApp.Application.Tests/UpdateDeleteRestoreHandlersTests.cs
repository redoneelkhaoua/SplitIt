using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Application.Customers.Commands.DeleteCustomer;
using TailoringApp.Application.Customers.Commands.RestoreCustomer;
using TailoringApp.Application.Customers.Commands.UpdateCustomer;
using TailoringApp.Domain.Customers;
using TailoringApp.Domain.Customers.ValueObjects;
using Xunit;

namespace TailoringApp.Application.Tests;

public class UpdateDeleteRestoreHandlersTests
{
    [Fact]
    public async Task UpdateCustomer_Should_ReturnFalse_When_NotFound()
    {
        var repo = new FakeCustomerRepository();
        var handler = new UpdateCustomerCommandHandler(repo);
        var cmd = new UpdateCustomerCommand(Guid.NewGuid(), "Jane", "Doe", null, "jane@example.com", null, null, null, null, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateCustomer_Should_Update_Fields_And_ReturnTrue()
    {
        var repo = new FakeCustomerRepository();
        var customer = CreateCustomer("CUST-1", "John", "Smith", "john@old.com");
        repo.AddSeed(customer);
        var handler = new UpdateCustomerCommandHandler(repo);
        var cmd = new UpdateCustomerCommand(customer.Id, "Johnny", "Smith", null, "john@new.com", "123", "Address", "Slim", "Modern", "Cotton");

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Should().BeTrue();
        var updated = await repo.GetByIdAsync(customer.Id);
        updated!.PersonalInfo.FirstName.Should().Be("Johnny");
        updated.ContactInfo.Email.Should().Be("john@new.com");
    updated.Preferences.Fit.Should().Be("Slim");
    }

    [Fact]
    public async Task DeleteCustomer_Should_SoftDelete_And_ReturnTrue()
    {
        var repo = new FakeCustomerRepository();
        var customer = CreateCustomer("CUST-2", "Alice", "Blue", "alice@example.com");
        repo.AddSeed(customer);
        var handler = new DeleteCustomerCommandHandler(repo);

        var result = await handler.Handle(new DeleteCustomerCommand(customer.Id), CancellationToken.None);

        result.Should().BeTrue();
        (await repo.GetByIdIncludingDeletedAsync(customer.Id))!.Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCustomer_Should_ReturnFalse_When_NotFound()
    {
        var repo = new FakeCustomerRepository();
        var handler = new DeleteCustomerCommandHandler(repo);
        var result = await handler.Handle(new DeleteCustomerCommand(Guid.NewGuid()), CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreCustomer_Should_Restore_And_ReturnTrue()
    {
        var repo = new FakeCustomerRepository();
        var customer = CreateCustomer("CUST-3", "Bob", "Brown", "bob@example.com");
        customer.SoftDelete();
        repo.AddSeed(customer);
        var handler = new RestoreCustomerCommandHandler(repo);

        var result = await handler.Handle(new RestoreCustomerCommand(customer.Id), CancellationToken.None);

        result.Should().BeTrue();
        (await repo.GetByIdIncludingDeletedAsync(customer.Id))!.Enabled.Should().BeTrue();
    }

    private static Customer CreateCustomer(string number, string first, string last, string email)
    {
        var personal = new PersonalInfo(first, last, null);
        var contact = new ContactInfo(email, string.Empty, string.Empty);
    var prefs = new CustomerPreferences(string.Empty, string.Empty, null);
        return new Customer(number, personal, contact, prefs);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        private readonly ConcurrentDictionary<Guid, Customer> _store = new();

        public Task AddAsync(Customer customer, CancellationToken ct = default)
        {
            _store[customer.Id] = customer;
            return Task.CompletedTask;
        }

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            _store.TryGetValue(id, out var c);
            return Task.FromResult(c);
        }

        public Task<Customer?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
        {
            _store.TryGetValue(id, out var c);
            return Task.FromResult(c);
        }

        public Task<Customer?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default)
        {
            _store.TryGetValue(id, out var c);
            return Task.FromResult(c);
        }

        public Task<Customer?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        {
            _store.TryGetValue(id, out var c);
            return Task.FromResult(c);
        }

        public Task<(IReadOnlyList<Customer> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, string? sortBy, bool sortDesc, string status, CancellationToken ct = default)
        {
            var items = _store.Values.ToList();
            return Task.FromResult(((IReadOnlyList<Customer>)items, items.Count));
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public void AddSeed(Customer c) => _store[c.Id] = c;
    }
}
