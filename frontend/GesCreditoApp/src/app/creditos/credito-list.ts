import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ClientesService, CreditosService, mensajeError } from '../core/api.service';
import { Cliente, Credito, ESTADOS, EstadoCredito } from '../core/models';

@Component({
  selector: 'app-credito-list',
  imports: [FormsModule, RouterLink, CurrencyPipe, DatePipe],
  template: `
    <div class="page-header">
      <h1>Créditos</h1>
      <a class="btn primary" routerLink="/creditos/nuevo">+ Nuevo crédito</a>
    </div>

    <div class="toolbar">
      <select [(ngModel)]="clienteId" (ngModelChange)="buscar()">
        <option [ngValue]="null">Todos los clientes</option>
        @for (c of clientes(); track c.id) {
          <option [ngValue]="c.id">{{ c.nombres }} {{ c.apellidos }}</option>
        }
      </select>
      <select [(ngModel)]="estado" (ngModelChange)="buscar()">
        <option value="">Todos los estados</option>
        @for (e of estados; track e) {
          <option [value]="e">{{ e }}</option>
        }
      </select>
    </div>

    @if (error()) {
      <p class="alert error">{{ error() }}</p>
    }

    <table>
      <thead>
        <tr>
          <th>Número</th><th>Cliente</th><th>Monto</th><th>Tasa %</th><th>Plazo</th>
          <th>Cuota</th><th>Estado</th><th>Solicitud</th><th></th>
        </tr>
      </thead>
      <tbody>
        @for (cr of creditos(); track cr.id) {
          <tr>
            <td>{{ cr.numero }}</td>
            <td><a [routerLink]="['/clientes', cr.clienteId]">{{ cr.clienteNombre }}</a></td>
            <td>{{ cr.monto | currency: 'COP' : 'symbol-narrow' : '1.0-0' }}</td>
            <td>{{ cr.tasaInteresAnual }}</td>
            <td>{{ cr.plazoMeses }}</td>
            <td>{{ cr.cuotaMensual | currency: 'COP' : 'symbol-narrow' : '1.0-0' }}</td>
            <td><span class="badge estado-{{ cr.estado }}">{{ cr.estado }}</span></td>
            <td>{{ cr.fechaSolicitud | date: 'dd/MM/yyyy' }}</td>
            <td class="actions">
              <a class="btn small" [routerLink]="['/creditos', cr.id, 'editar']">Editar</a>
              <button class="btn small danger" (click)="eliminar(cr)">Eliminar</button>
            </td>
          </tr>
        } @empty {
          <tr><td colspan="9" class="empty">Sin resultados.</td></tr>
        }
      </tbody>
    </table>

    <div class="pager">
      <button class="btn small" [disabled]="page() <= 1" (click)="ir(page() - 1)">«</button>
      <span>Página {{ page() }} de {{ totalPaginas() }} ({{ total() }} registros)</span>
      <button class="btn small" [disabled]="page() >= totalPaginas()" (click)="ir(page() + 1)">»</button>
    </div>
  `,
})
export class CreditoList implements OnInit {
  private readonly api = inject(CreditosService);
  private readonly clientesApi = inject(ClientesService);
  private readonly pageSize = 10;

  protected readonly estados = ESTADOS;
  protected clienteId: number | null = null;
  protected estado: EstadoCredito | '' = '';

  protected readonly clientes = signal<Cliente[]>([]);
  protected readonly creditos = signal<Credito[]>([]);
  protected readonly page = signal(1);
  protected readonly total = signal(0);
  protected readonly error = signal('');

  protected totalPaginas(): number {
    return Math.max(1, Math.ceil(this.total() / this.pageSize));
  }

  ngOnInit(): void {
    this.clientesApi.listar('', 1, 100).subscribe((r) => this.clientes.set(r.items));
    this.cargar();
  }

  protected buscar(): void {
    this.page.set(1);
    this.cargar();
  }

  protected ir(p: number): void {
    this.page.set(p);
    this.cargar();
  }

  protected eliminar(cr: Credito): void {
    if (!confirm(`¿Eliminar el crédito ${cr.numero}?`)) return;
    this.api.eliminar(cr.id).subscribe({
      next: () => this.cargar(),
      error: (e) => this.error.set(mensajeError(e)),
    });
  }

  private cargar(): void {
    this.error.set('');
    this.api
      .listar({ clienteId: this.clienteId, estado: this.estado }, this.page(), this.pageSize)
      .subscribe({
        next: (r) => {
          this.creditos.set(r.items);
          this.total.set(r.total);
        },
        error: (e) => this.error.set(mensajeError(e)),
      });
  }
}
