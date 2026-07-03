namespace InvoiceToCash.Models;

public enum InvoiceStatus
{
    Pending,
    Paid,
    Overdue
}

public record Invoice
{
    public int Id { get; init; }
    public string PartnerName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "PLN";
    public InvoiceStatus Status { get; init; }
    public DateOnly DueDate { get; init; }
}
