using InvoiceToCash.Models;

namespace InvoiceToCash.Data;

public interface IInvoiceRepository
{
    IReadOnlyList<Invoice> GetAll();
    IReadOnlyList<Invoice> GetByStatus(InvoiceStatus status);
}

/// <summary>
/// In-memory repository seeded with sample data.
/// For a demo this is sufficient; in production this would be an EF Core / Cosmos-backed repository.
/// </summary>
public class InMemoryInvoiceRepository : IInvoiceRepository
{
    private readonly List<Invoice> _invoices =
    [
        new() { Id = 1, PartnerName = "Northwind Traders", Amount = 12500.00m, Currency = "PLN", Status = InvoiceStatus.Paid,    DueDate = new DateOnly(2026, 6, 15) },
        new() { Id = 2, PartnerName = "Contoso Ltd",       Amount = 8400.50m,  Currency = "PLN", Status = InvoiceStatus.Pending, DueDate = new DateOnly(2026, 7, 20) },
        new() { Id = 3, PartnerName = "Fabrikam Inc",      Amount = 23100.00m, Currency = "PLN", Status = InvoiceStatus.Paid,    DueDate = new DateOnly(2026, 6, 30) },
        new() { Id = 4, PartnerName = "Adventure Works",   Amount = 5600.00m,  Currency = "PLN", Status = InvoiceStatus.Overdue, DueDate = new DateOnly(2026, 5, 10) },
        new() { Id = 5, PartnerName = "Wide World Imports", Amount = 17800.75m, Currency = "PLN", Status = InvoiceStatus.Paid,   DueDate = new DateOnly(2026, 7, 5) },
        new() { Id = 6, PartnerName = "Tailspin Toys",     Amount = 3200.00m,  Currency = "PLN", Status = InvoiceStatus.Pending, DueDate = new DateOnly(2026, 8, 1) },
        new() { Id = 7, PartnerName = "Proseware Inc",     Amount = 9950.00m,  Currency = "PLN", Status = InvoiceStatus.Overdue, DueDate = new DateOnly(2026, 4, 28) },
        new() { Id = 8, PartnerName = "Coho Vineyard",     Amount = 14300.00m, Currency = "PLN", Status = InvoiceStatus.Paid,    DueDate = new DateOnly(2026, 6, 22) },
    ];

    public IReadOnlyList<Invoice> GetAll() => _invoices;

    public IReadOnlyList<Invoice> GetByStatus(InvoiceStatus status) =>
        _invoices.Where(i => i.Status == status).ToList();
}
