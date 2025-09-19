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

  // Verificar permissões baseadas no tipo de usuário
  if (currentRoute.startsWith('/dashboard') || currentRoute.startsWith('/collaborator')) {
    if (!userProfileService.isGestor()) {
      const defaultRoute = userProfileService.getDefaultRoute();
      router.navigate([defaultRoute]);
      return false;
    }
  } else if (currentRoute.startsWith('/home') || currentRoute.startsWith('/progresso') || currentRoute.startsWith('/topic')) {
    if (!userProfileService.isColaborador()) {
      const defaultRoute = userProfileService.getDefaultRoute();
      router.navigate([defaultRoute]);
      return false;
    }
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