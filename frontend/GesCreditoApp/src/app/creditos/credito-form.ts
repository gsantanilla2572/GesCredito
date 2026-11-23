import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ClientesService, CreditosService, mensajeError } from '../core/api.service';
import { Cliente, CreditoGuardar, ESTADOS, EstadoCredito, calcularCuota } from '../core/models';

@Component({
  selector: 'app-credito-form',
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe],
  template: `
    <h1>{{ id() ? 'Editar crédito' : 'Nuevo crédito' }}</h1>

    @if (error()) {
      <p class="alert error">{{ error() }}</p>
    }

    <form class="form" [formGroup]="form" (ngSubmit)="guardar()">
      <label>
        Cliente
        <select formControlName="clienteId">
          <option [ngValue]="null" disabled>Seleccione…</option>
          @for (c of clientes(); track c.id) {
            <option [ngValue]="c.id">{{ c.identificacion }} — {{ c.nombres }} {{ c.apellidos }}</option>
          }
        </select>
        @if (invalido('clienteId')) {
          <small class="err">Seleccione un cliente.</small>
        }
      </label>
      <label>
        Monto (COP)
        <input type="number" formControlName="monto" min="1" />
        @if (invalido('monto')) {
          <small class="err">Debe ser mayor que 0.</small>
        }
      </label>
      <label>
        Tasa de interés anual (%)
        <input type="number" formControlName="tasaInteresAnual" min="0" max="100" step="0.01" />
        @if (invalido('tasaInteresAnual')) {
          <small class="err">Entre 0 y 100.</small>
        }
      </label>
      <label>
        Plazo (meses)
        <input type="number" formControlName="plazoMeses" min="1" max="360" />
        @if (invalido('plazoMeses')) {
          <small class="err">Entre 1 y 360 meses.</small>
        }
      </label>

      @if (id()) {
        <label>
          Estado
          <select formControlName="estado">
            @for (e of estados; track e) {
              <option [ngValue]="e">{{ e }}</option>
            }
          </select>
        </label>
      }

      <label>
        Observaciones
        <textarea formControlName="observaciones" rows="3" maxlength="500"></textarea>
      </label>

      <p class="card">
        Cuota mensual estimada:
        <strong>{{ cuota() | currency: 'COP' : 'symbol-narrow' : '1.0-0' }}</strong>
      </p>

      <div class="actions">
        <button class="btn primary" type="submit" [disabled]="guardando()">Guardar</button>
        <a class="btn" routerLink="/creditos">Cancelar</a>
      </div>
    </form>
  `,
})
export class CreditoForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(CreditosService);
  private readonly clientesApi = inject(ClientesService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly estados = ESTADOS;
  protected readonly id = signal<number | null>(null);
  protected readonly clientes = signal<Cliente[]>([]);
  protected readonly error = signal('');
  protected readonly guardando = signal(false);
  protected readonly cuota = signal(0);

  protected readonly form = this.fb.group({
    clienteId: this.fb.control<number | null>(null, Validators.required),
    monto: this.fb.control<number | null>(null, [Validators.required, Validators.min(1)]),
    tasaInteresAnual: this.fb.control<number | null>(12, [
      Validators.required,
      Validators.min(0),
      Validators.max(100),
    ]),
    plazoMeses: this.fb.control<number | null>(12, [
      Validators.required,
      Validators.min(1),
      Validators.max(360),
    ]),
    estado: this.fb.control<EstadoCredito | null>('Solicitado'),
    observaciones: [''],
  });

  ngOnInit(): void {
    this.clientesApi.listar('', 1, 100).subscribe((r) => this.clientes.set(r.items));

    this.form.valueChanges.subscribe((v) =>
      this.cuota.set(calcularCuota(v.monto ?? 0, v.tasaInteresAnual ?? 0, v.plazoMeses ?? 0)),
    );

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.id.set(Number(idParam));
      this.api.obtener(Number(idParam)).subscribe({
        next: (c) =>
          this.form.patchValue({
            clienteId: c.clienteId,
            monto: c.monto,
            tasaInteresAnual: c.tasaInteresAnual,
            plazoMeses: c.plazoMeses,
            estado: c.estado,
            observaciones: c.observaciones ?? '',
          }),
        error: (e) => this.error.set(mensajeError(e)),
      });
    } else {
      const clienteId = this.route.snapshot.queryParamMap.get('clienteId');
      if (clienteId) this.form.patchValue({ clienteId: Number(clienteId) });
    }
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
    const dto: CreditoGuardar = {
      clienteId: v.clienteId!,
      monto: v.monto!,
      tasaInteresAnual: v.tasaInteresAnual!,
      plazoMeses: v.plazoMeses!,
      estado: this.id() ? v.estado : null,
      observaciones: v.observaciones || null,
    };
    this.guardando.set(true);
    this.error.set('');
    const id = this.id();
    const req = id ? this.api.actualizar(id, dto) : this.api.crear(dto);
    req.subscribe({
      next: () => this.router.navigate(['/creditos']),
      error: (e) => {
        this.error.set(mensajeError(e));
        this.guardando.set(false);
      },
    });
  }
}
