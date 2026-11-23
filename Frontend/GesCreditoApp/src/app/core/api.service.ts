import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  Cliente,
  ClienteDetalle,
  ClienteGuardar,
  Credito,
  CreditoGuardar,
  EstadoCredito,
  PagedResult,
} from './models';

export const API_URL = '/api';

@Injectable({ providedIn: 'root' })
export class ClientesService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/clientes`;

  listar(search = '', page = 1, pageSize = 10): Observable<PagedResult<Cliente>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<Cliente>>(this.url, { params });
  }

  obtener(id: number): Observable<ClienteDetalle> {
    return this.http.get<ClienteDetalle>(`${this.url}/${id}`);
  }

  crear(dto: ClienteGuardar): Observable<Cliente> {
    return this.http.post<Cliente>(this.url, dto);
  }

  actualizar(id: number, dto: ClienteGuardar): Observable<Cliente> {
    return this.http.put<Cliente>(`${this.url}/${id}`, dto);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}

@Injectable({ providedIn: 'root' })
export class CreditosService {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/creditos`;

  listar(
    filtros: { clienteId?: number | null; estado?: EstadoCredito | '' } = {},
    page = 1,
    pageSize = 10,
  ): Observable<PagedResult<Credito>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (filtros.clienteId) params = params.set('clienteId', filtros.clienteId);
    if (filtros.estado) params = params.set('estado', filtros.estado);
    return this.http.get<PagedResult<Credito>>(this.url, { params });
  }

  obtener(id: number): Observable<Credito> {
    return this.http.get<Credito>(`${this.url}/${id}`);
  }

  crear(dto: CreditoGuardar): Observable<Credito> {
    return this.http.post<Credito>(this.url, dto);
  }

  actualizar(id: number, dto: CreditoGuardar): Observable<Credito> {
    return this.http.put<Credito>(`${this.url}/${id}`, dto);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}

/** Extrae un mensaje legible de un ProblemDetails / error de validación del API. */
export function mensajeError(err: unknown): string {
  const e = err as { error?: { detail?: string; title?: string; errors?: Record<string, string[]> } };
  if (e?.error?.errors) return Object.values(e.error.errors).flat().join(' ');
  return e?.error?.detail ?? e?.error?.title ?? 'Ocurrió un error inesperado.';
}
