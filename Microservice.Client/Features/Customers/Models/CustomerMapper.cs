namespace Microservice.Client.Features.Customers.Models;

/// <summary>Explicit DTO↔ViewModel mapping for Customers (archetype-consistent, no reflection).</summary>
public static class CustomerMapper
{
    public static CustomerListItemVm ToListItem(GetCustomersPaginatedDto dto) =>
        new(dto.PublicId, dto.Name, dto.DocType, dto.DocNumber, dto.TaxCondition, dto.IsActive);

    public static CustomerListItemVm ToListItem(GetCustomerDto dto) =>
        new(dto.PublicId, dto.Name, dto.DocType, dto.DocNumber, dto.TaxCondition, dto.IsActive);

    public static CustomerFormModel ToFormModel(GetCustomerDto dto) => new()
    {
        PublicId = dto.PublicId,
        Name = dto.Name,
        DocType = dto.DocType,
        DocNumber = dto.DocNumber,
        TaxCondition = dto.TaxCondition,
        Email = dto.Email,
        Phone = dto.Phone,
        Address = dto.Address,
        IsActive = dto.IsActive
    };

    public static CreateCustomerRequest ToCreateRequest(CustomerFormModel m) => new(
        Name: m.Name.Trim(),
        DocType: m.DocType,
        DocNumber: m.DocNumber.Trim(),
        TaxCondition: m.TaxCondition,
        Email: string.IsNullOrWhiteSpace(m.Email) ? null : m.Email.Trim(),
        Phone: string.IsNullOrWhiteSpace(m.Phone) ? null : m.Phone.Trim(),
        Address: string.IsNullOrWhiteSpace(m.Address) ? null : m.Address.Trim());

    /// <summary>Full edit view → sends all scalar fields (the form is a complete edit surface).</summary>
    public static UpdateCustomerRequest ToUpdateRequest(CustomerFormModel m) => new(
        Name: m.Name.Trim(),
        DocType: m.DocType,
        DocNumber: m.DocNumber.Trim(),
        TaxCondition: m.TaxCondition,
        Email: string.IsNullOrWhiteSpace(m.Email) ? null : m.Email.Trim(),
        Phone: string.IsNullOrWhiteSpace(m.Phone) ? null : m.Phone.Trim(),
        Address: string.IsNullOrWhiteSpace(m.Address) ? null : m.Address.Trim());
}
