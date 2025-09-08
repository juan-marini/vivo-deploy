import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(c => c.LoginComponent)
  },
//   {
//     path: 'dashboard',
//     loadComponent: () => import('./features/dashboard/dashboard.component').then(c => c.DashboardComponent),
//     canActivate: [authGuard]
//   },
//   {
//     path: 'onboarding',
//     loadComponent: () => import('./features/onboarding/onboarding.component').then(c => c.OnboardingComponent),
//     canActivate: [authGuard]
//   },
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