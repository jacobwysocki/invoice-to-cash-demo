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

    public InvoicesController(IInvoiceRepository repository, ICommissionService commissionService)
    {
        _repository = repository;
        _commissionService = commissionService;
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
