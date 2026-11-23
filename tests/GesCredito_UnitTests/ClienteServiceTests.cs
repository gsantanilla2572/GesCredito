using GesCredito.Domain.Entities;
using GesCredito.Domain.Enums;
using GesCredito.Infrastructure.Data;
using GesCredito.Infrastructure.Repositories;
using GesCredito.Services.Dtos;
using GesCredito.Services.Exceptions;
using GesCredito.Services.Implementations;

namespace GesCredito.UnitTests;

public class ClienteServiceTests
{
    private static (ClienteService Service, GesCreditoDbContext Db) Crear()
    {
        var db = TestDb.Create();
        return (new ClienteService(new ClienteRepository(db), db), db);
    }

    private static ClienteGuardarDto Dto(string id = "123", string nombres = "ANA") => new()
    {
        Identificacion = id, Nombres = nombres, Apellidos = "PÉREZ", Email = "ana@example.com"
    };

    [Fact]
    public async Task Crear_PersisteYDevuelveDto()
    {
        var (service, db) = Crear();

        var creado = await service.CrearAsync(Dto());

        Assert.True(creado.Id > 0);
        Assert.Equal("123", creado.Identificacion);
        Assert.Equal(1, db.Clientes.Count());
    }

    [Fact]
    public async Task Crear_IdentificacionDuplicada_LanzaConflict()
    {
        var (service, _) = Crear();
        await service.CrearAsync(Dto());

        await Assert.ThrowsAsync<ConflictException>(() => service.CrearAsync(Dto(nombres: "Otra")));
    }

    [Fact]
    public async Task Actualizar_ModificaCampos()
    {
        var (service, _) = Crear();
        var creado = await service.CrearAsync(Dto());
        var cambios = Dto(nombres: "ANA MARÍA");
        cambios.Activo = false;

        var actualizado = await service.ActualizarAsync(creado.Id, cambios);

        Assert.Equal("ANA MARÍA", actualizado.Nombres);
        Assert.False(actualizado.Activo);
    }

    [Fact]
    public async Task Actualizar_IdentificacionDeOtroCliente_LanzaConflict()
    {
        var (service, _) = Crear();
        await service.CrearAsync(Dto("111"));
        var segundo = await service.CrearAsync(Dto("222"));

        await Assert.ThrowsAsync<ConflictException>(() => service.ActualizarAsync(segundo.Id, Dto("111")));
    }

    [Fact]
    public async Task Obtener_Inexistente_LanzaNotFound()
    {
        var (service, _) = Crear();
        await Assert.ThrowsAsync<NotFoundException>(() => service.ObtenerAsync(999));
    }

    [Fact]
    public async Task Obtener_IncluyeSusCreditos()
    {
        var (service, db) = Crear();
        var cliente = await service.CrearAsync(Dto());
        db.Creditos.Add(new Credito
        {
            Numero = "CR-1", ClienteId = cliente.Id, Monto = 1000, TasaInteresAnual = 10,
            PlazoMeses = 6, CuotaMensual = 170, Estado = EstadoCredito.Solicitado
        });
        await db.SaveChangesAsync();

        var detalle = await service.ObtenerAsync(cliente.Id);

        Assert.Single(detalle.Creditos);
        Assert.Equal("CR-1", detalle.Creditos[0].Numero);
        Assert.Equal(1, detalle.CantidadCreditos);
    }

    [Fact]
    public async Task Listar_FiltraPorTextoYPagina()
    {
        var (service, _) = Crear();
        await service.CrearAsync(Dto("1", "ANA"));
        await service.CrearAsync(Dto("2", "BEATRIZ"));
        await service.CrearAsync(Dto("3", "CARLOS"));

        var filtrado = await service.ListarAsync("BEA", 1, 10);
        var paginado = await service.ListarAsync(null, 1, 2);

        Assert.Single(filtrado.Items);
        Assert.Equal(2, paginado.Items.Count);
        Assert.Equal(3, paginado.Total);
    }

    [Fact]
    public async Task Eliminar_SinCreditos_Elimina()
    {
        var (service, db) = Crear();
        var cliente = await service.CrearAsync(Dto());

        await service.EliminarAsync(cliente.Id);

        Assert.Empty(db.Clientes);
    }

    [Fact]
    public async Task Eliminar_ConCreditos_LanzaConflict()
    {
        var (service, db) = Crear();
        var cliente = await service.CrearAsync(Dto());
        db.Creditos.Add(new Credito { Numero = "CR-2", ClienteId = cliente.Id, Monto = 500, PlazoMeses = 3, CuotaMensual = 170 });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => service.EliminarAsync(cliente.Id));
    }
	[Fact]
	public async Task Crear_NormalizaTextoAMayusculas()
	{
		var (service, _) = Crear();
		var c = await service.CrearAsync(new ClienteGuardarDto
		{
			Identificacion = " ab-1 ", Nombres = "sergio iván", Apellidos = "mora pardo",
			Email = "Sergio@Example.com", Direccion = "calle 5"
		});

		Assert.Equal("AB-1", c.Identificacion);
		Assert.Equal("SERGIO IVÁN", c.Nombres);
		Assert.Equal("MORA PARDO", c.Apellidos);
		Assert.Equal("sergio@example.com", c.Email);
		Assert.Equal("CALLE 5", c.Direccion);
	}
}
