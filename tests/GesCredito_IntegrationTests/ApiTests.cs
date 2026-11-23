using System.Net;
using System.Net.Http.Json;
using GesCredito.Services.Dtos;

namespace GesCredito.IntegrationTests;

public class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _http = factory.CreateClient();

    private static ClienteGuardarDto NuevoCliente(string id) => new()
    {
        Identificacion = id, Nombres = "Ana", Apellidos = "Pérez", Email = $"{id}@example.com"
    };

    private async Task<ClienteDto> CrearCliente(string id)
    {
        var r = await _http.PostAsJsonAsync("/api/clientes", NuevoCliente(id), ApiFactory.Json);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        return (await r.Content.ReadFromJsonAsync<ClienteDto>(ApiFactory.Json))!;
    }

    private Task<HttpResponseMessage> CrearCredito(int clienteId, decimal monto = 1_000_000) =>
        _http.PostAsJsonAsync("/api/creditos", new CreditoGuardarDto
        {
            ClienteId = clienteId, Monto = monto, TasaInteresAnual = 12, PlazoMeses = 12
        }, ApiFactory.Json);

    [Fact]
    public async Task Health_Responde200()
    {
        var r = await _http.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Cliente_CrudCompleto()
    {
        var creado = await CrearCliente("it-crud");

        var get = await _http.GetFromJsonAsync<ClienteDetalleDto>($"/api/clientes/{creado.Id}", ApiFactory.Json);
        Assert.Equal("IT-CRUD", get!.Identificacion);

        var cambios = NuevoCliente("it-crud");
        cambios.Nombres = "Ana María";
        var put = await _http.PutAsJsonAsync($"/api/clientes/{creado.Id}", cambios, ApiFactory.Json);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var del = await _http.DeleteAsync($"/api/clientes/{creado.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        var notFound = await _http.GetAsync($"/api/clientes/{creado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task Cliente_Duplicado_Devuelve409()
    {
        await CrearCliente("it-dup");
        var r = await _http.PostAsJsonAsync("/api/clientes", NuevoCliente("it-dup"), ApiFactory.Json);
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
    }

    [Fact]
    public async Task Cliente_EmailInvalido_Devuelve400()
    {
        var dto = NuevoCliente("it-bad");
        dto.Email = "no-es-un-email";
        var r = await _http.PostAsJsonAsync("/api/clientes", dto, ApiFactory.Json);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Credito_CrearYConsultarPorCliente()
    {
        var cliente = await CrearCliente("it-cred");

        var r = await CrearCredito(cliente.Id);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var credito = (await r.Content.ReadFromJsonAsync<CreditoDto>(ApiFactory.Json))!;
        Assert.Equal(88_848.79m, credito.CuotaMensual);

        var lista = await _http.GetFromJsonAsync<PagedResult<CreditoDto>>(
            $"/api/creditos?clienteId={cliente.Id}&estado=Solicitado", ApiFactory.Json);
        Assert.Single(lista!.Items);

        var detalle = await _http.GetFromJsonAsync<ClienteDetalleDto>($"/api/clientes/{cliente.Id}", ApiFactory.Json);
        Assert.Single(detalle!.Creditos);
    }

    [Fact]
    public async Task Credito_ClienteInexistente_Devuelve404()
    {
        var r = await CrearCredito(clienteId: 99999);
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task Credito_FlujoDeEstados_Y_EliminacionBloqueada()
    {
        var cliente = await CrearCliente("it-flow");
        var credito = (await (await CrearCredito(cliente.Id)).Content.ReadFromJsonAsync<CreditoDto>(ApiFactory.Json))!;

        CreditoGuardarDto Cambio(GesCredito.Domain.Enums.EstadoCredito estado) => new()
        {
            ClienteId = cliente.Id, Monto = credito.Monto, TasaInteresAnual = credito.TasaInteresAnual,
            PlazoMeses = credito.PlazoMeses, Estado = estado
        };

        var salto = await _http.PutAsJsonAsync($"/api/creditos/{credito.Id}",
            Cambio(GesCredito.Domain.Enums.EstadoCredito.Pagado), ApiFactory.Json);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, salto.StatusCode);

        foreach (var e in new[] { GesCredito.Domain.Enums.EstadoCredito.Aprobado, GesCredito.Domain.Enums.EstadoCredito.Desembolsado })
        {
            var ok = await _http.PutAsJsonAsync($"/api/creditos/{credito.Id}", Cambio(e), ApiFactory.Json);
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        }

        var del = await _http.DeleteAsync($"/api/creditos/{credito.Id}");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, del.StatusCode);
    }

    [Fact]
    public async Task Cliente_ConCreditos_NoSePuedeEliminar()
    {
        var cliente = await CrearCliente("it-del");
        await CrearCredito(cliente.Id);

        var r = await _http.DeleteAsync($"/api/clientes/{cliente.Id}");

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
    }
}
