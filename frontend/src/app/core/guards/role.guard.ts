import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { UserProfileService } from '../services/user-profile.service';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const userProfileService = inject(UserProfileService);
  const router = inject(Router);

  // Verificar se está autenticado
  if (!authService.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }

  const currentRoute = state.url;
  const userRole = userProfileService.getUserRole();

  // Regras de acesso por rota
  const routePermissions = {
    '/dashboard': ['Administrador', 'Gestor'],
    '/home': ['Colaborador'],
    '/progresso': ['Colaborador'],
    '/topic': ['Colaborador']
  };

  // Verificar se a rota tem permissões definidas
  const allowedRoles = routePermissions[currentRoute as keyof typeof routePermissions];

  if (allowedRoles && !allowedRoles.includes(userRole)) {
    // Redirecionar para a rota padrão do usuário
    const defaultRoute = userProfileService.getDefaultRoute();
    router.navigate([defaultRoute]);
    return false;
  }

  return true;
};

export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const userProfileService = inject(UserProfileService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }

  if (!userProfileService.isAdmin()) {
    const defaultRoute = userProfileService.getDefaultRoute();
    router.navigate([defaultRoute]);
    return false;
  }

  return true;
};

export const gestorGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const userProfileService = inject(UserProfileService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }

  if (!userProfileService.isGestor()) {
    const defaultRoute = userProfileService.getDefaultRoute();
    router.navigate([defaultRoute]);
    return false;
  }

  return true;
};

export const colaboradorGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const userProfileService = inject(UserProfileService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }

  if (!userProfileService.isColaborador()) {
    const defaultRoute = userProfileService.getDefaultRoute();
    router.navigate([defaultRoute]);
    return false;
  }

  return true;
};