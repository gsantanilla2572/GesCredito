namespace GesCredito.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;

    public ICollection<Credito> Creditos { get; set; } = new List<Credito>();

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
}
