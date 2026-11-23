import { calcularCuota } from './models';

describe('calcularCuota', () => {
  it('devuelve 0 con datos inválidos', () => {
    expect(calcularCuota(0, 12, 12)).toBe(0);
    expect(calcularCuota(1000, 12, 0)).toBe(0);
  });

  it('divide en partes iguales con tasa 0', () => {
    expect(calcularCuota(1200, 0, 12)).toBe(100);
  });

  it('calcula la cuota por sistema francés', () => {
    // 10.000.000 al 12% anual a 12 meses => 888.487,89
    expect(calcularCuota(10_000_000, 12, 12)).toBeCloseTo(888487.89, 1);
  });
});
