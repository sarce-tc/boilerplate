namespace Microservice.Domain.Entities;

/// <summary>Tipo de documento de identidad/fiscal del cliente (AFIP/ARCA).</summary>
public enum DocumentType
{
    /// <summary>Documento Nacional de Identidad.</summary>
    Dni = 0,

    /// <summary>Clave Única de Identificación Tributaria.</summary>
    Cuit = 1,

    /// <summary>Código Único de Identificación Laboral.</summary>
    Cuil = 2,

    /// <summary>Pasaporte u otro documento extranjero.</summary>
    Passport = 3,
}
