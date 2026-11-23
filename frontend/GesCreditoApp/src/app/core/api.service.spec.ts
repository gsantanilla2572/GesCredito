import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ClientesService, CreditosService, mensajeError } from './api.service';

describe('API services', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('ClientesService.listar envía búsqueda y paginación', () => {
    TestBed.inject(ClientesService).listar('ana', 2, 5).subscribe();
    const req = http.expectOne((r) => r.url === '/api/clientes');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('search')).toBe('ana');
    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('5');
    req.flush({ items: [], total: 0, page: 2, pageSize: 5 });
  });

  it('CreditosService.listar aplica filtros', () => {
    TestBed.inject(CreditosService).listar({ clienteId: 3, estado: 'Aprobado' }).subscribe();
    const req = http.expectOne((r) => r.url === '/api/creditos');
    expect(req.request.params.get('clienteId')).toBe('3');
    expect(req.request.params.get('estado')).toBe('Aprobado');
    req.flush({ items: [], total: 0, page: 1, pageSize: 10 });
  });

  it('CreditosService.eliminar usa DELETE', () => {
    TestBed.inject(CreditosService).eliminar(9).subscribe();
    const req = http.expectOne('/api/creditos/9');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('mensajeError prioriza el detalle del ProblemDetails', () => {
    expect(mensajeError({ error: { detail: 'Cliente con créditos activos' } })).toBe(
      'Cliente con créditos activos',
    );
    expect(mensajeError({})).toContain('inesperado');
  });
});
