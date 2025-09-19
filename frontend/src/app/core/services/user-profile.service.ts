import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { Usuario } from '../models/auth.model';

export enum UserRole {
  ADMIN = 'Administrador',
  COLABORADOR = 'Colaborador',
  GESTOR = 'Gestor'
}

export interface NavigationItem {
  label: string;
  route: string;
  icon: string;
  visible: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class UserProfileService {
  private currentUser: Usuario | null = null;

  constructor(private authService: AuthService) {
    this.authService.currentUser.subscribe(user => {
      this.currentUser = user;
    });
  }

  getCurrentUser(): Usuario | null {
    return this.currentUser;
  }

  getUserRole(): string {
    return this.currentUser?.perfil || UserRole.COLABORADOR;
  }

  isAdmin(): boolean {
    const role = this.getUserRole();
    return role === UserRole.ADMIN || role === 'Administrador';
  }

  isGestor(): boolean {
    const role = this.getUserRole();
    return role === UserRole.GESTOR ||
           role === 'Gestão' ||
           this.isAdmin();
  }

  isColaborador(): boolean {
    const role = this.getUserRole();
    return role === UserRole.COLABORADOR ||
           role === 'Desenvolvimento' ||
           role === 'Infraestrutura' ||
           role === 'QA' ||
           role === 'Produto' ||
           role === 'Dados' ||
           role === 'Design';
  }

  getWelcomeMessage(): string {
    const user = this.getCurrentUser();
    if (!user) return 'Seja bem-vindo(a)';

    const firstName = user.nomeCompleto.split(' ')[0];

    return `Seja bem-vindo(a), ${firstName}`;
  }

  getNavigationItems(): NavigationItem[] {
    const isGestorOrAdmin = this.isGestor();
    const isColaborador = this.isColaborador();

    return [
      {
        label: 'Home',
        route: '/home',
        icon: 'fas fa-home',
        visible: isColaborador
      },
      {
        label: 'Meu Progresso',
        route: '/progresso',
        icon: 'fas fa-chart-line',
        visible: isColaborador
      },
      {
        label: 'Dashboard',
        route: '/dashboard',
        icon: 'fas fa-tachometer-alt',
        visible: isGestorOrAdmin
      }
    ].filter(item => item.visible);
  }

  getDefaultRoute(): string {
    if (this.isGestor()) {
      return '/dashboard';
    }
    return '/home';
  }

  canAccessRoute(route: string): boolean {
    const navigationItems = this.getNavigationItems();
    return navigationItems.some(item => item.route === route);
  }

  hasPermission(permission: string): boolean {
    const role = this.getUserRole();

    const permissions = {
      [UserRole.ADMIN]: ['dashboard', 'users', 'reports', 'home', 'progress'],
      [UserRole.GESTOR]: ['dashboard', 'reports'],
      [UserRole.COLABORADOR]: ['home', 'progress', 'topics']
    };

    return permissions[role as keyof typeof permissions]?.includes(permission) || false;
  }

  getAvailableRoutes(): string[] {
    if (this.isGestor()) {
      return ['/dashboard'];
    }
    return ['/home', '/progresso', '/topic'];
  }
}