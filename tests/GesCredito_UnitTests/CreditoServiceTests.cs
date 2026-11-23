using GesCredito.Domain.Contracts;
using GesCredito.Domain.Entities;
using GesCredito.Domain.Enums;
using GesCredito.Services.Dtos;
using GesCredito.Services.Exceptions;
using GesCredito.Services.Implementations;
using Moq;

namespace GesCredito.UnitTests;

public class CreditoServiceTests
{
    private readonly Mock<ICreditoRepository> _creditos = new();
    private readonly Mock<IClienteRepository> _clientes = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly CreditoService _service;

    public CreditoServiceTests() =>
        _service = new CreditoService(_creditos.Object, _clientes.Object, _uow.Object);

    private static Cliente ClienteActivo(int id = 1) => new()
    {
        Id = id, Identificacion = "123", Nombres = "Ana", Apellidos = "Pérez", Email = "a@a.com", Activo = true
    };

    private static Credito CreditoExistente(EstadoCredito estado, int id = 10) => new()
    {
        Id = id, Numero = "CR-X", ClienteId = 1, Cliente = ClienteActivo(), Monto = 1_000_000,
        TasaInteresAnual = 12, PlazoMeses = 12, CuotaMensual = 88_848.79m, Estado = estado
    };

    private static CreditoGuardarDto Dto(EstadoCredito? estado = null) => new()
    {
        ClienteId = 1, Monto = 1_000_000, TasaInteresAnual = 12, PlazoMeses = 12, Estado = estado
    };

    private void SetupCliente(Cliente? cliente) =>
        _clientes.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(cliente);

    private void SetupCredito(Credito? credito) =>
        _creditos.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(credito);

    [Fact]
    public async Task Crear_CalculaCuota_IniciaSolicitadoYGuarda()
    {
        SetupCliente(ClienteActivo());

        var dto = await _service.CrearAsync(Dto(EstadoCredito.Pagado)); // el estado enviado se ignora

        Assert.Equal(EstadoCredito.Solicitado, dto.Estado);
        Assert.Equal(88_848.79m, dto.CuotaMensual);
        Assert.StartsWith("CR-", dto.Numero);
        _creditos.Verify(r => r.AddAsync(It.IsAny<Credito>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Crear_ClienteInexistente_LanzaNotFound()
    {
        SetupCliente(null);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CrearAsync(Dto()));
    }

    [Fact]
    public async Task Crear_ClienteInactivo_LanzaBusinessRule()
    {
        var inactivo = ClienteActivo();
        inactivo.Activo = false;
        SetupCliente(inactivo);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearAsync(Dto()));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0, 12, 12)]
    [InlineData(1000, 101, 12)]
    [InlineData(1000, 12, 361)]
    public async Task Crear_CondicionesInvalidas_LanzaBusinessRule(decimal monto, decimal tasa, int plazo)
    {
        SetupCliente(ClienteActivo());
        var dto = Dto();
        dto.Monto = monto; dto.TasaInteresAnual = tasa; dto.PlazoMeses = plazo;

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CrearAsync(dto));
    }

    [Fact]
    public async Task Actualizar_Inexistente_LanzaNotFound()
    {
        SetupCredito(null);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ActualizarAsync(1, Dto()));
    }

    [Theory]
    [InlineData(EstadoCredito.Pagado)]
    [InlineData(EstadoCredito.Rechazado)]
    [InlineData(EstadoCredito.Cancelado)]
    public async Task Actualizar_EstadoFinal_LanzaBusinessRule(EstadoCredito estado)
    {
        SetupCredito(CreditoExistente(estado));
        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ActualizarAsync(10, Dto()));
    }

    [Fact]
    public async Task Actualizar_Solicitado_RecalculaCuota()
    {
        SetupCredito(CreditoExistente(EstadoCredito.Solicitado));
        var dto = Dto();
        dto.PlazoMeses = 24;

        var r = await _service.ActualizarAsync(10, dto);

        Assert.Equal(24, r.PlazoMeses);
        Assert.Equal(47_073.47m, r.CuotaMensual);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Actualizar_CambiarMontoEnAprobado_LanzaBusinessRule()
    {
        SetupCredito(CreditoExistente(EstadoCredito.Aprobado));
        var dto = Dto();
        dto.Monto = 2_000_000;

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ActualizarAsync(10, dto));
    }

    [Fact]
    public async Task Actualizar_TransicionInvalida_LanzaBusinessRule()
    {
        SetupCredito(CreditoExistente(EstadoCredito.Solicitado));
        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ActualizarAsync(10, Dto(EstadoCredito.Pagado)));
    }

    [Fact]
    public async Task Actualizar_AprobadoADesembolsado_FijaFechaDesembolso()
    {
        SetupCredito(CreditoExistente(EstadoCredito.Aprobado));

        var r = await _service.ActualizarAsync(10, Dto(EstadoCredito.Desembolsado));

        Assert.Equal(EstadoCredito.Desembolsado, r.Estado);
        Assert.NotNull(r.FechaDesembolso);
    }

    [Fact]
    public async Task Eliminar_Desembolsado_LanzaBusinessRule()
    {
        SetupCredito(CreditoExistente(EstadoCredito.Desembolsado));
        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.EliminarAsync(10));
        _creditos.Verify(r => r.Remove(It.IsAny<Credito>()), Times.Never);
    }

    [Fact]
    public async Task Eliminar_Solicitado_Elimina()
    {
        var credito = CreditoExistente(EstadoCredito.Solicitado);
        SetupCredito(credito);

        await _service.EliminarAsync(10);

        _creditos.Verify(r => r.Remove(credito), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Listar_NormalizaPaginacion()
    {
        _creditos.Setup(r => r.BuscarAsync(null, null, 1, 100, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((new List<Credito>(), 0));

		var r = await _service.ListarAsync(null, null, 0, 5000);

        Assert.Equal(1, r.Page);
        Assert.Equal(100, r.PageSize);
    }
}
