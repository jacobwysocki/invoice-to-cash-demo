using InvoiceToCash.Data;
using InvoiceToCash.Models;
using InvoiceToCash.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceToCash.Controllers;

[ApiController]
[Route("[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceRepository _repository;
    private readonly ICommissionService _commissionService;
    private readonly IAgingService _agingService;

    public InvoicesController(
        IInvoiceRepository repository,
        ICommissionService commissionService,
        IAgingService agingService)
    {
        _repository = repository;
        _commissionService = commissionService;
        _agingService = agingService;
    }

    /// <summary>Returns all invoices, each with its calculated commission.</summary>
    [HttpGet]
    public ActionResult<IEnumerable<InvoiceDto>> GetAll()
    {
        var result = _repository.GetAll()
            .Select(i => new InvoiceDto(
                i.Id, i.PartnerName, i.Amount, i.Currency, i.Status.ToString(), i.DueDate,
                _commissionService.CalculateCommission(i)));
        return Ok(result);
    }

    /// <summary>Returns only overdue invoices.</summary>
    [HttpGet("overdue")]
    public ActionResult<IEnumerable<InvoiceDto>> GetOverdue()
    {
        var result = _repository.GetByStatus(InvoiceStatus.Overdue)
            .Select(i => new InvoiceDto(
                i.Id, i.PartnerName, i.Amount, i.Currency, i.Status.ToString(), i.DueDate,
                _commissionService.CalculateCommission(i)));
        return Ok(result);
    }

    /// <summary>Returns the total commission across all paid invoices.</summary>
    [HttpGet("commission-total")]
    public ActionResult<CommissionTotalDto> GetCommissionTotal()
    {
        var total = _commissionService.CalculateTotalCommission();
        return Ok(new CommissionTotalDto(total, "PLN"));
    }

    /// <summary>Returns unpaid invoices grouped into aging buckets by days past their due date.</summary>
    [HttpGet("aging")]
    public ActionResult<AgingReportDto> GetAging()
    {
        var buckets = _agingService.CalculateAging()
            .Select(b => new AgingBucketDto(b.Label, b.Count, b.TotalAmount))
            .ToList();
        return Ok(new AgingReportDto(buckets, "PLN"));
    }
}

// DTOs — API contract, decoupled from the domain entity.
public record InvoiceDto(
    int Id,
    string PartnerName,
    decimal Amount,
    string Currency,
    string Status,
    DateOnly DueDate,
    decimal Commission);

public record CommissionTotalDto(decimal Total, string Currency);

public record AgingBucketDto(string Label, int Count, decimal TotalAmount);

public record AgingReportDto(IReadOnlyList<AgingBucketDto> Buckets, string Currency);
