namespace GesCredito.Services.Exceptions;

/// <summary>El recurso solicitado no existe (HTTP 404).</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>Conflicto con el estado actual de los datos, p. ej. duplicados (HTTP 409).</summary>
public class ConflictException(string message) : Exception(message);

/// <summary>Se viola una regla de negocio (HTTP 422).</summary>
public class BusinessRuleException(string message) : Exception(message);
