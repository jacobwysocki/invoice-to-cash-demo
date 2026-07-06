using InvoiceToCash.Data;
using InvoiceToCash.Models;

namespace InvoiceToCash.Services;

public interface IAgingService
{
    IReadOnlyList<AgingBucket> CalculateAging();
}

/// <summary>A single aging bucket: how many unpaid invoices fall into it and their combined value.</summary>
public record AgingBucket(string Label, int Count, decimal TotalAmount);

/// <summary>
/// Invoice aging logic. Bucketing rule lives ONLY here (single source of truth).
/// Unpaid invoices (Pending + Overdue) are grouped by how many days past their DueDate
/// they are relative to today. Paid invoices are excluded.
/// </summary>
public class AgingService : IAgingService
{
    // Bucket labels, in report order. "Current" means not yet due (0 or fewer days past due).
    private const string Current = "Current";
    private const string Days1To30 = "1-30 days";
    private const string Days31To60 = "31-60 days";
    private const string Days61To90 = "61-90 days";
    private const string Days90Plus = "90+ days";

    private readonly IInvoiceRepository _repository;
    private readonly TimeProvider _timeProvider;

    public AgingService(IInvoiceRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public IReadOnlyList<AgingBucket> CalculateAging()
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        // Fixed set of buckets, always returned in this order (empty buckets included).
        var counts = new Dictionary<string, (int Count, decimal Total)>
        {
            [Current] = (0, 0m),
            [Days1To30] = (0, 0m),
            [Days31To60] = (0, 0m),
            [Days61To90] = (0, 0m),
            [Days90Plus] = (0, 0m),
        };

        foreach (var invoice in _repository.GetAll())
        {
            if (invoice.Status == InvoiceStatus.Paid)
            {
                continue; // Only unpaid invoices age.
            }

            var daysPastDue = today.DayNumber - invoice.DueDate.DayNumber;
            var label = BucketFor(daysPastDue);

            var (count, total) = counts[label];
            counts[label] = (count + 1, total + invoice.Amount);
        }

        return counts
            .Select(kvp => new AgingBucket(kvp.Key, kvp.Value.Count, kvp.Value.Total))
            .ToList();
    }

    private static string BucketFor(int daysPastDue) => daysPastDue switch
    {
        <= 0 => Current,
        <= 30 => Days1To30,
        <= 60 => Days31To60,
        <= 90 => Days61To90,
        _ => Days90Plus,
    };
}
