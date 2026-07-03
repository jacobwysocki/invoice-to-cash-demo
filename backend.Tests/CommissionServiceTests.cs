using InvoiceToCash.Data;
using InvoiceToCash.Models;
using InvoiceToCash.Services;
using Xunit;

namespace InvoiceToCash.Tests;

public class CommissionServiceTests
{
    private static Invoice MakeInvoice(decimal amount, InvoiceStatus status) => new()
    {
        Id = 1,
        PartnerName = "Test Partner",
        Amount = amount,
        Currency = "PLN",
        Status = status,
        DueDate = new DateOnly(2026, 1, 1)
    };

    private static CommissionService MakeService(params Invoice[] invoices)
    {
        return new CommissionService(new StubRepository(invoices));
    }

    [Fact]
    public void PaidInvoice_ReturnsTwoAndHalfPercent()
    {
        var service = MakeService();
        var invoice = MakeInvoice(10000m, InvoiceStatus.Paid);

        var commission = service.CalculateCommission(invoice);

        Assert.Equal(250m, commission);
    }

    [Theory]
    [InlineData(InvoiceStatus.Pending)]
    [InlineData(InvoiceStatus.Overdue)]
    public void NonPaidInvoice_ReturnsZero(InvoiceStatus status)
    {
        var service = MakeService();
        var invoice = MakeInvoice(10000m, status);

        var commission = service.CalculateCommission(invoice);

        Assert.Equal(0m, commission);
    }

    [Fact]
    public void ZeroAmount_ReturnsZeroCommission()
    {
        var service = MakeService();
        var invoice = MakeInvoice(0m, InvoiceStatus.Paid);

        var commission = service.CalculateCommission(invoice);

        Assert.Equal(0m, commission);
    }

    [Fact]
    public void LargeAmount_RoundsToTwoDecimals()
    {
        var service = MakeService();
        var invoice = MakeInvoice(999999.99m, InvoiceStatus.Paid);

        var commission = service.CalculateCommission(invoice);

        // 999999.99 * 0.025 = 24999.99975 -> rounds to 25000.00
        Assert.Equal(25000.00m, commission);
    }

    [Fact]
    public void TotalCommission_SumsOnlyPaidInvoices()
    {
        var service = MakeService(
            MakeInvoice(10000m, InvoiceStatus.Paid),    // 250
            MakeInvoice(20000m, InvoiceStatus.Paid),    // 500
            MakeInvoice(5000m, InvoiceStatus.Pending),  // 0
            MakeInvoice(8000m, InvoiceStatus.Overdue)); // 0

        var total = service.CalculateTotalCommission();

        Assert.Equal(750m, total);
    }

    // Minimal stub so tests don't depend on the seeded in-memory data.
    private sealed class StubRepository : IInvoiceRepository
    {
        private readonly IReadOnlyList<Invoice> _invoices;
        public StubRepository(IReadOnlyList<Invoice> invoices) => _invoices = invoices;
        public IReadOnlyList<Invoice> GetAll() => _invoices;
        public IReadOnlyList<Invoice> GetByStatus(InvoiceStatus status) =>
            _invoices.Where(i => i.Status == status).ToList();
    }
}
