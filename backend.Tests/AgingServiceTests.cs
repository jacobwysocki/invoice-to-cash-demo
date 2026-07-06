using InvoiceToCash.Data;
using InvoiceToCash.Models;
using InvoiceToCash.Services;
using Xunit;

namespace InvoiceToCash.Tests;

public class AgingServiceTests
{
    // Fixed "today" so bucket boundaries are deterministic.
    private static readonly DateOnly Today = new(2026, 7, 3);

    private static AgingService MakeService(params Invoice[] invoices)
    {
        return new AgingService(new StubRepository(invoices), new FakeTimeProvider(Today));
    }

    // Builds an unpaid (Pending) invoice due `daysPastDue` days before today.
    // Negative daysPastDue means the invoice is not yet due.
    private static Invoice DueDaysPastDue(int daysPastDue, decimal amount = 1000m, InvoiceStatus status = InvoiceStatus.Pending) => new()
    {
        Id = 1,
        PartnerName = "Test Partner",
        Amount = amount,
        Currency = "PLN",
        Status = status,
        DueDate = Today.AddDays(-daysPastDue)
    };

    private static AgingBucket Bucket(IReadOnlyList<AgingBucket> buckets, string label) =>
        buckets.Single(b => b.Label == label);

    [Fact]
    public void AlwaysReturnsFiveBucketsInOrder()
    {
        var buckets = MakeService().CalculateAging();

        Assert.Equal(
            new[] { "Current", "1-30 days", "31-60 days", "61-90 days", "90+ days" },
            buckets.Select(b => b.Label).ToArray());
    }

    [Fact]
    public void EmptyBuckets_HaveZeroCountAndZeroTotal()
    {
        var buckets = MakeService().CalculateAging();

        Assert.All(buckets, b =>
        {
            Assert.Equal(0, b.Count);
            Assert.Equal(0m, b.TotalAmount);
        });
    }

    [Theory]
    [InlineData(-5, "Current")]   // due in the future
    [InlineData(0, "Current")]    // due exactly today -> not yet overdue
    [InlineData(1, "1-30 days")]  // first day past due
    [InlineData(30, "1-30 days")] // exactly 30 days -> still 1-30
    [InlineData(31, "31-60 days")]// exactly 31 days -> tips into next bucket
    [InlineData(60, "31-60 days")]
    [InlineData(61, "61-90 days")]
    [InlineData(90, "61-90 days")]// exactly 90 days -> still 61-90
    [InlineData(91, "90+ days")]  // 91+ -> 90+ bucket
    [InlineData(365, "90+ days")]
    public void InvoiceLandsInExpectedBucket(int daysPastDue, string expectedLabel)
    {
        var buckets = MakeService(DueDaysPastDue(daysPastDue)).CalculateAging();

        Assert.Equal(1, Bucket(buckets, expectedLabel).Count);
        // Every other bucket stays empty.
        Assert.All(buckets.Where(b => b.Label != expectedLabel), b => Assert.Equal(0, b.Count));
    }

    [Theory]
    [InlineData(InvoiceStatus.Pending)]
    [InlineData(InvoiceStatus.Overdue)]
    public void UnpaidStatuses_AreIncluded(InvoiceStatus status)
    {
        var buckets = MakeService(DueDaysPastDue(45, status: status)).CalculateAging();

        Assert.Equal(1, Bucket(buckets, "31-60 days").Count);
    }

    [Fact]
    public void PaidInvoices_AreExcludedFromEveryBucket()
    {
        // A paid invoice long past due must not appear anywhere.
        var buckets = MakeService(DueDaysPastDue(100, status: InvoiceStatus.Paid)).CalculateAging();

        Assert.All(buckets, b => Assert.Equal(0, b.Count));
    }

    [Fact]
    public void MultipleInvoicesInSameBucket_CountAndAmountAggregate()
    {
        var buckets = MakeService(
            DueDaysPastDue(10, amount: 1000m),
            DueDaysPastDue(20, amount: 2500m),
            DueDaysPastDue(50, amount: 9999m)) // different bucket, must not leak in
            .CalculateAging();

        var oneToThirty = Bucket(buckets, "1-30 days");
        Assert.Equal(2, oneToThirty.Count);
        Assert.Equal(3500m, oneToThirty.TotalAmount);

        Assert.Equal(1, Bucket(buckets, "31-60 days").Count);
        Assert.Equal(9999m, Bucket(buckets, "31-60 days").TotalAmount);
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

    // Fixed-clock TimeProvider so "today" is deterministic (no external package needed).
    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;
        public FakeTimeProvider(DateOnly today) =>
            _now = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => _now;
    }
}
