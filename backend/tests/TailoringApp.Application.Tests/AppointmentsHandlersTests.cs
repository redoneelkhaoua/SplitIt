using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Application.Appointments.Commands.CancelAppointment;
using TailoringApp.Application.Appointments.Commands.RescheduleAppointment;
using TailoringApp.Application.Appointments.Commands.ScheduleAppointment;
using TailoringApp.Application.Appointments.Commands.CompleteAppointment;
using TailoringApp.Domain.Appointments;
using TailoringApp.Domain.Customers;
using TailoringApp.Domain.Customers.ValueObjects;
using Xunit;

namespace TailoringApp.Application.Tests;

public class AppointmentsHandlersTests
{
    [Fact]
    public async Task Schedule_Should_Create_When_NoConflict_And_CustomerExists()
    {
        var appts = new FakeAppointmentRepository();
        var customers = new FakeCustomerRepository();
        var customer = CreateCustomer("C-1", "Alice", "Blue", "a@b.com");
        await customers.AddAsync(customer);
        var handler = new ScheduleAppointmentCommandHandler(appts, customers);

        var start = DateTime.UtcNow.AddHours(1);
        var end = start.AddHours(1);
        var id = await handler.Handle(new ScheduleAppointmentCommand(customer.Id, start, end, "first"), CancellationToken.None);

        id.Should().NotBe(Guid.Empty);
        (await appts.GetByIdAsync(id))!.CustomerId.Should().Be(customer.Id);
    }

    [Fact]
    public async Task Schedule_Should_ReturnEmpty_When_CustomerMissing()
    {
        var appts = new FakeAppointmentRepository();
        var customers = new FakeCustomerRepository();
        var handler = new ScheduleAppointmentCommandHandler(appts, customers);

        var start = DateTime.UtcNow.AddHours(1);
        var end = start.AddHours(1);
        var id = await handler.Handle(new ScheduleAppointmentCommand(Guid.NewGuid(), start, end, null), CancellationToken.None);

        id.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task Schedule_Should_ReturnEmpty_When_Conflicts()
    {
        var appts = new FakeAppointmentRepository();
        var customers = new FakeCustomerRepository();
        var customer = CreateCustomer("C-2", "Bob", "Brown", "b@c.com");
        await customers.AddAsync(customer);
        // Seed conflicting appointment
        await appts.AddAsync(new Appointment(customer.Id, DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(3), null));
        var handler = new ScheduleAppointmentCommandHandler(appts, customers);

        var start = DateTime.UtcNow.AddHours(2.5);
        var end = start.AddHours(1);
        var id = await handler.Handle(new ScheduleAppointmentCommand(customer.Id, start, end, null), CancellationToken.None);

        id.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task Reschedule_Should_Move_When_NoConflict_And_OwnedByCustomer()
    {
        var appts = new FakeAppointmentRepository();
        var customers = new FakeCustomerRepository();
        var customer = CreateCustomer("C-3", "Cara", "Green", "c@d.com");
        await customers.AddAsync(customer);
        var appt = new Appointment(customer.Id, DateTime.UtcNow.AddHours(4), DateTime.UtcNow.AddHours(5), null);
        await appts.AddAsync(appt);
        var handler = new RescheduleAppointmentCommandHandler(appts, customers);

    var ok = await handler.Handle(new RescheduleAppointmentCommand(appt.Id, customer.Id, DateTime.UtcNow.AddHours(6), DateTime.UtcNow.AddHours(7)), CancellationToken.None);

        ok.Should().BeTrue();
        var stored = await appts.GetByIdAsync(appt.Id);
        stored!.StartUtc.Should().BeCloseTo(DateTime.UtcNow.AddHours(6), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Reschedule_Should_Fail_When_Conflict()
    {
        var appts = new FakeAppointmentRepository();
        var customers = new FakeCustomerRepository();
        var customer = CreateCustomer("C-4", "Dan", "White", "d@e.com");
        await customers.AddAsync(customer);
        var a1 = new Appointment(customer.Id, DateTime.UtcNow.AddHours(8), DateTime.UtcNow.AddHours(9), null);
        var a2 = new Appointment(customer.Id, DateTime.UtcNow.AddHours(10), DateTime.UtcNow.AddHours(11), null);
        await appts.AddAsync(a1);
        await appts.AddAsync(a2);
        var handler = new RescheduleAppointmentCommandHandler(appts, customers);

    var ok = await handler.Handle(new RescheduleAppointmentCommand(a2.Id, customer.Id, DateTime.UtcNow.AddHours(8.5), DateTime.UtcNow.AddHours(9.5)), CancellationToken.None);

        ok.Should().BeFalse();
    }

    [Fact]
    public async Task Cancel_Should_Succeed_When_OwnerMatches()
    {
        var appts = new FakeAppointmentRepository();
        var customers = new FakeCustomerRepository();
        var customer = CreateCustomer("C-5", "Eve", "Black", "e@f.com");
        await customers.AddAsync(customer);
        var appt = new Appointment(customer.Id, DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2), null);
        await appts.AddAsync(appt);
        var handler = new CancelAppointmentCommandHandler(appts);

        var ok = await handler.Handle(new CancelAppointmentCommand(appt.Id, customer.Id), CancellationToken.None);

        ok.Should().BeTrue();
    }

    [Fact]
    public async Task Complete_Should_Succeed_When_OwnerMatches_And_Scheduled()
    {
        var appts = new FakeAppointmentRepository();
        var customers = new FakeCustomerRepository();
        var customer = CreateCustomer("C-6", "Fay", "Grey", "f@g.com");
        await customers.AddAsync(customer);
        var appt = new Appointment(customer.Id, DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2), null);
        await appts.AddAsync(appt);
        var handler = new CompleteAppointmentCommandHandler(appts);

        var ok = await handler.Handle(new CompleteAppointmentCommand(appt.Id, customer.Id), CancellationToken.None);

        ok.Should().BeTrue();
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
        public Task<Customer?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default) => GetByIdAsync(id, ct);
        public Task<Customer?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default) => GetByIdAsync(id, ct);
        public Task<Customer?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) => GetByIdAsync(id, ct);
        public Task<(IReadOnlyList<Customer> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, string? sortBy, bool sortDesc, string status, CancellationToken ct = default)
        {
            var items = _store.Values.ToList();
            return Task.FromResult(((IReadOnlyList<Customer>)items, items.Count));
        }
        public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeAppointmentRepository : IAppointmentRepository
    {
        private readonly ConcurrentDictionary<Guid, Appointment> _store = new();

        public Task AddAsync(Appointment appt, CancellationToken ct = default)
        {
            _store[appt.Id] = appt;
            return Task.CompletedTask;
        }

        public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            _store.TryGetValue(id, out var a);
            return Task.FromResult(a);
        }

        public Task<(IReadOnlyList<Appointment> Items, int Total)> GetForCustomerAsync(Guid customerId, DateTime? fromUtc, DateTime? toUtc, int page, int pageSize, CancellationToken ct = default)
        {
            var q = _store.Values.Where(a => a.CustomerId == customerId);
            if (fromUtc.HasValue) q = q.Where(a => a.EndUtc >= fromUtc.Value);
            if (toUtc.HasValue) q = q.Where(a => a.StartUtc <= toUtc.Value);
            var items = q.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult(((IReadOnlyList<Appointment>)items, q.Count()));
        }

        public Task<bool> HasConflictAsync(Guid customerId, DateTime startUtc, DateTime endUtc, Guid? excludeId = null, CancellationToken ct = default)
        {
            var conflict = _store.Values.Any(a => a.CustomerId == customerId && (!excludeId.HasValue || a.Id != excludeId.Value)
                && a.StartUtc < endUtc && startUtc < a.EndUtc);
            return Task.FromResult(conflict);
        }

        public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
    }
}
