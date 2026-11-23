import { Routes } from '@angular/router';
import { ClienteDetail } from './clientes/cliente-detail';
import { ClienteForm } from './clientes/cliente-form';
import { ClienteList } from './clientes/cliente-list';
import { CreditoForm } from './creditos/credito-form';
import { CreditoList } from './creditos/credito-list';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'clientes' },
  { path: 'clientes', component: ClienteList, title: 'Clientes' },
  { path: 'clientes/nuevo', component: ClienteForm, title: 'Nuevo cliente' },
  { path: 'clientes/:id', component: ClienteDetail, title: 'Detalle de cliente' },
  { path: 'clientes/:id/editar', component: ClienteForm, title: 'Editar cliente' },
  { path: 'creditos', component: CreditoList, title: 'Créditos' },
  { path: 'creditos/nuevo', component: CreditoForm, title: 'Nuevo crédito' },
  { path: 'creditos/:id/editar', component: CreditoForm, title: 'Editar crédito' },
  { path: '**', redirectTo: 'clientes' },
];
