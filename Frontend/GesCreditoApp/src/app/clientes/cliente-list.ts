import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ClientesService, mensajeError } from '../core/api.service';
import { Cliente } from '../core/models';

@Component({
  selector: 'app-cliente-list',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="page-header">
      <h1>Clientes</h1>
      <a class="btn primary" routerLink="/clientes/nuevo">+ Nuevo cliente</a>
    </div>

    <form class="toolbar" (ngSubmit)="buscar()">
      <input
        name="search"
        [(ngModel)]="search"
        placeholder="Buscar por nombre, identificación o email"
      />
      <button class="btn" type="submit">Buscar</button>
    </form>

    @if (error()) {
      <p class="alert error">{{ error() }}</p>
    }

    <table>
      <thead>
        <tr>
          <th>Identificación</th>
          <th>Nombre</th>
          <th>Email</th>
          <th>Teléfono</th>
          <th>Créditos</th>
          <th>Estado</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        @for (c of clientes(); track c.id) {
          <tr>
            <td>{{ c.identificacion }}</td>
            <td>{{ c.nombres }} {{ c.apellidos }}</td>
            <td>{{ c.email }}</td>
            <td>{{ c.telefono }}</td>
            <td>{{ c.cantidadCreditos }}</td>
            <td>
              <span class="badge" [class.off]="!c.activo">{{ c.activo ? 'Activo' : 'Inactivo' }}</span>
            </td>
            <td class="actions">
              <a class="btn small" [routerLink]="['/clientes', c.id]">Ver</a>
              <a class="btn small" [routerLink]="['/clientes', c.id, 'editar']">Editar</a>
              <button class="btn small danger" (click)="eliminar(c)">Eliminar</button>
            </td>
          </tr>
        } @empty {
          <tr>
            <td colspan="7" class="empty">Sin resultados.</td>
          </tr>
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
export class ClienteList implements OnInit {
  private readonly api = inject(ClientesService);
  private readonly pageSize = 10;

  protected search = '';
  protected readonly clientes = signal<Cliente[]>([]);
  protected readonly page = signal(1);
  protected readonly total = signal(0);
  protected readonly error = signal('');

  protected totalPaginas(): number {
    return Math.max(1, Math.ceil(this.total() / this.pageSize));
  }

  ngOnInit(): void {
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

  protected eliminar(c: Cliente): void {
    if (!confirm(`¿Eliminar al cliente ${c.nombres} ${c.apellidos}?`)) return;
    this.api.eliminar(c.id).subscribe({
      next: () => this.cargar(),
      error: (e) => this.error.set(mensajeError(e)),
    });
  }

  private cargar(): void {
    this.error.set('');
    this.api.listar(this.search, this.page(), this.pageSize).subscribe({
      next: (r) => {
        this.clientes.set(r.items);
        this.total.set(r.total);
      },
      error: (e) => this.error.set(mensajeError(e)),
    });
  }
}
