import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ClientesService, mensajeError } from '../core/api.service';
import { ClienteGuardar } from '../core/models';

@Component({
  selector: 'app-cliente-form',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <h1>{{ id() ? 'Editar cliente' : 'Nuevo cliente' }}</h1>

    @if (error()) {
      <p class="alert error">{{ error() }}</p>
    }

    <form class="form" [formGroup]="form" (ngSubmit)="guardar()">
      <label>
        Identificación
        <input formControlName="identificacion" maxlength="20" />
        @if (invalido('identificacion')) {
          <small class="err">Requerida (máx. 20 caracteres).</small>
        }
      </label>
      <label>
        Nombres
        <input formControlName="nombres" maxlength="100" />
        @if (invalido('nombres')) {
          <small class="err">Requerido.</small>
        }
      </label>
      <label>
        Apellidos
        <input formControlName="apellidos" maxlength="100" />
        @if (invalido('apellidos')) {
          <small class="err">Requerido.</small>
        }
      </label>
      <label>
        Email
        <input formControlName="email" type="email" maxlength="150" />
        @if (invalido('email')) {
          <small class="err">Email válido requerido.</small>
        }
      </label>
      <label>
        Teléfono
        <input formControlName="telefono" maxlength="30" />
      </label>
      <label>
        Dirección
        <input formControlName="direccion" maxlength="200" />
      </label>
      <label class="check">
        <input type="checkbox" formControlName="activo" /> Cliente activo
      </label>

      <div class="actions">
        <button class="btn primary" type="submit" [disabled]="guardando()">Guardar</button>
        <a class="btn" routerLink="/clientes">Cancelar</a>
      </div>
    </form>
  `,
})
export class ClienteForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ClientesService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly id = signal<number | null>(null);
  protected readonly error = signal('');
  protected readonly guardando = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    identificacion: ['', [Validators.required, Validators.maxLength(20)]],
    nombres: ['', [Validators.required, Validators.maxLength(100)]],
    apellidos: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
    telefono: [''],
    direccion: [''],
    activo: [true],
  });

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam) return;
    this.id.set(Number(idParam));
    this.api.obtener(Number(idParam)).subscribe({
      next: (c) =>
        this.form.patchValue({
          identificacion: c.identificacion,
          nombres: c.nombres,
          apellidos: c.apellidos,
          email: c.email,
          telefono: c.telefono ?? '',
          direccion: c.direccion ?? '',
          activo: c.activo,
        }),
      error: (e) => this.error.set(mensajeError(e)),
    });
  }

  protected invalido(campo: string): boolean {
    const c = this.form.get(campo);
    return !!c && c.invalid && (c.dirty || c.touched);
  }

  protected guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const dto: ClienteGuardar = {
      ...v,
      telefono: v.telefono || null,
      direccion: v.direccion || null,
    };
    this.guardando.set(true);
    this.error.set('');
    const id = this.id();
    const req = id ? this.api.actualizar(id, dto) : this.api.crear(dto);
    req.subscribe({
      next: () => this.router.navigate(['/clientes']),
      error: (e) => {
        this.error.set(mensajeError(e));
        this.guardando.set(false);
      },
    });
  }
}
