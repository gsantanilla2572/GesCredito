import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ClientesService, CreditosService, mensajeError } from '../core/api.service';
import { ClienteDetalle, Credito } from '../core/models';

@Component({
  selector: 'app-cliente-detail',
  imports: [RouterLink, CurrencyPipe, DatePipe],
  template: `
    @if (error()) {
      <p class="alert error">{{ error() }}</p>
    }
    @if (cliente(); as c) {
      <div class="page-header">
        <h1>{{ c.nombres }} {{ c.apellidos }}</h1>
        <div>
          <a class="btn" [routerLink]="['/clientes', c.id, 'editar']">Editar</a>
          <a class="btn primary" routerLink="/creditos/nuevo" [queryParams]="{ clienteId: c.id }">
            + Nuevo crédito
          </a>
        </div>
      </div>

      <dl class="card">
        <dt>Identificación</dt><dd>{{ c.identificacion }}</dd>
        <dt>Email</dt><dd>{{ c.email }}</dd>
        <dt>Teléfono</dt><dd>{{ c.telefono || '—' }}</dd>
        <dt>Dirección</dt><dd>{{ c.direccion || '—' }}</dd>
        <dt>Registro</dt><dd>{{ c.fechaRegistro | date: 'dd/MM/yyyy' }}</dd>
        <dt>Estado</dt><dd>{{ c.activo ? 'Activo' : 'Inactivo' }}</dd>
      </dl>

      <h2>Créditos del cliente</h2>
      <table>
        <thead>
          <tr>
            <th>Número</th><th>Monto</th><th>Tasa % anual</th><th>Plazo</th>
            <th>Cuota</th><th>Estado</th><th></th>
          </tr>
        </thead>
        <tbody>
          @for (cr of c.creditos; track cr.id) {
            <tr>
              <td>{{ cr.numero }}</td>
              <td>{{ cr.monto | currency: 'COP' : 'symbol-narrow' : '1.0-0' }}</td>
              <td>{{ cr.tasaInteresAnual }}</td>
              <td>{{ cr.plazoMeses }} meses</td>
              <td>{{ cr.cuotaMensual | currency: 'COP' : 'symbol-narrow' : '1.0-0' }}</td>
              <td><span class="badge estado-{{ cr.estado }}">{{ cr.estado }}</span></td>
              <td class="actions">
                <a class="btn small" [routerLink]="['/creditos', cr.id, 'editar']">Editar</a>
                <button class="btn small danger" (click)="eliminarCredito(cr)">Eliminar</button>
              </td>
            </tr>
          } @empty {
            <tr><td colspan="7" class="empty">El cliente no tiene créditos.</td></tr>
          }
        </tbody>
      </table>
      <p><a routerLink="/clientes">← Volver a clientes</a></p>
    }
  `,
})
export class ClienteDetail implements OnInit {
  private readonly clientes = inject(ClientesService);
  private readonly creditos = inject(CreditosService);
  private readonly route = inject(ActivatedRoute);

  protected readonly cliente = signal<ClienteDetalle | null>(null);
  protected readonly error = signal('');

  ngOnInit(): void {
    this.cargar();
  }

  protected eliminarCredito(cr: Credito): void {
    if (!confirm(`¿Eliminar el crédito ${cr.numero}?`)) return;
    this.creditos.eliminar(cr.id).subscribe({
      next: () => this.cargar(),
      error: (e) => this.error.set(mensajeError(e)),
    });
  }

  private cargar(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.clientes.obtener(id).subscribe({
      next: (c) => this.cliente.set(c),
      error: (e) => this.error.set(mensajeError(e)),
    });
  }
}
