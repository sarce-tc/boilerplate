namespace Microservice.Client.Features.Customers.Models;

/// <summary>Row model for the customers grid.</summary>
public sealed record CustomerListItemVm(
    Guid PublicId,
    string Name,
    DocumentType DocType,
    string DocNumber,
    TaxCondition TaxCondition,
    bool IsActive);

/// <summary>Mutable create/edit model bound by CustomerForm.</summary>
public sealed class CustomerFormModel
{
    public Guid? PublicId { get; set; }   // null = create
    public string Name { get; set; } = string.Empty;
    public DocumentType DocType { get; set; } = DocumentType.Dni;
    public string DocNumber { get; set; } = string.Empty;
    public TaxCondition TaxCondition { get; set; } = TaxCondition.ConsumidorFinal;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsNew => PublicId is null;
}

/// <summary>Display labels for the AFIP enums (single source for UI text).</summary>
public static class CustomerLabels
{
    public static string Doc(DocumentType t) => t switch
    {
        DocumentType.Dni => "DNI",
        DocumentType.Cuit => "CUIT",
        DocumentType.Cuil => "CUIL",
        DocumentType.Passport => "Pasaporte",
        _ => t.ToString()
    };

    public static string Tax(TaxCondition t) => t switch
    {
        TaxCondition.ConsumidorFinal => "Consumidor final",
        TaxCondition.ResponsableInscripto => "Responsable inscripto",
        TaxCondition.Monotributista => "Monotributista",
        TaxCondition.Exento => "Exento",
        TaxCondition.NoResponsable => "No responsable",
        _ => t.ToString()
    };
}
