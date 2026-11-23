export type EstadoCredito =
  | 'Solicitado'
  | 'Aprobado'
  | 'Desembolsado'
  | 'Pagado'
  | 'Rechazado'
  | 'Cancelado';

export const ESTADOS: EstadoCredito[] = [
  'Solicitado',
  'Aprobado',
  'Desembolsado',
  'Pagado',
  'Rechazado',
  'Cancelado',
];

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface Cliente {
  id: number;
  identificacion: string;
  nombres: string;
  apellidos: string;
  email: string;
  telefono: string | null;
  direccion: string | null;
  fechaRegistro: string;
  activo: boolean;
  cantidadCreditos: number;
}

export interface ClienteDetalle extends Cliente {
  creditos: Credito[];
}

export interface ClienteGuardar {
  identificacion: string;
  nombres: string;
  apellidos: string;
  email: string;
  telefono: string | null;
  direccion: string | null;
  activo: boolean;
}

export interface Credito {
  id: number;
  numero: string;
  clienteId: number;
  clienteNombre: string;
  monto: number;
  tasaInteresAnual: number;
  plazoMeses: number;
  cuotaMensual: number;
  totalAPagar: number;
  estado: EstadoCredito;
  fechaSolicitud: string;
  fechaDesembolso: string | null;
  observaciones: string | null;
}

export interface CreditoGuardar {
  clienteId: number;
  monto: number;
  tasaInteresAnual: number;
  plazoMeses: number;
  estado: EstadoCredito | null;
  observaciones: string | null;
}

/** Cuota mensual (sistema francés). Debe coincidir con la del backend. */
export function calcularCuota(monto: number, tasaAnual: number, plazoMeses: number): number {
  if (!monto || !plazoMeses || monto <= 0 || plazoMeses <= 0) return 0;
  const r = tasaAnual / 12 / 100;
  const cuota = r === 0 ? monto / plazoMeses : (monto * r) / (1 - Math.pow(1 + r, -plazoMeses));
  return Math.round(cuota * 100) / 100;
}
