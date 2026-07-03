using InvoiceToCash.Data;
using InvoiceToCash.Models;

namespace InvoiceToCash.Services;

public interface ICommissionService
{
    decimal CalculateCommission(Invoice invoice);
    decimal CalculateTotalCommission();
}

/// <summary>
/// Commission logic. Business rule lives ONLY here (single source of truth).
/// Rule: commission = 2.5% of Amount for Paid invoices, 0 otherwise.
/// </summary>
public class CommissionService : ICommissionService
{
    private const decimal CommissionRate = 0.025m;

    private readonly IInvoiceRepository _repository;

    public CommissionService(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public decimal CalculateCommission(Invoice invoice)
    {
        // Only Paid invoices generate commission.
        return invoice.Status == InvoiceStatus.Paid
            ? Math.Round(invoice.Amount * CommissionRate, 2)
            : 0m;
    }

    public decimal CalculateTotalCommission()
    {
        return _repository.GetAll().Sum(CalculateCommission);
    }
}
