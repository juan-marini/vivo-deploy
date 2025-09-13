import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(c => c.LoginComponent)
  },
  {
    path: 'home',
    loadComponent: () => import('./features/home/home.component').then(c => c.HomeComponent)
    // canActivate: [authGuard] // Removido para desenvolvimento
  },
  {
    path: 'progresso',
    loadComponent: () => import('./features/progresso/progresso.component').then(c => c.ProgressoComponent)
    // canActivate: [authGuard] // Removido para desenvolvimento
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(c => c.DashboardComponent)
    // canActivate: [authGuard] // Removido para desenvolvimento
  },
  {
    path: 'topic/:id',
    loadComponent: () => import('./features/topic/topic.component').then(c => c.TopicComponent)
    // canActivate: [authGuard] // Removido para desenvolvimento
  },
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: '/login'
  }
];