import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { UserProfileService, NavigationItem } from '../../../core/services/user-profile.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <header class="header">
      <div class="header-container">
        <!-- Brand Section -->
        <div class="brand-section">
          <div class="vivo-brand">
            <div class="vivo-logo">
              <span class="logo-text">VIVO</span>
            </div>
            <div class="brand-info">
              <h1 class="welcome-title">{{ welcomeMessage }}</h1>
              <span class="system-subtitle">Sistema de Onboarding</span>
            </div>
          </div>
        </div>

        <!-- Navigation Section -->
        <div class="nav-section">
          <nav class="main-navigation">
            <a
              *ngFor="let item of navigationItems"
              [routerLink]="item.route"
              class="nav-item"
              routerLinkActive="nav-item--active">
              <i [class]="item.icon"></i>
              <span>{{ item.label }}</span>
            </a>
          </nav>

          <div class="user-actions">
            <button class="logout-btn" (click)="onLogout()">
              <i class="fas fa-sign-out-alt"></i>
              <span>Sair</span>
            </button>
          </div>
        </div>
      </div>
    </header>
  `,
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {
  welcomeMessage = '';
  navigationItems: NavigationItem[] = [];

  constructor(
    private userProfileService: UserProfileService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.welcomeMessage = this.userProfileService.getWelcomeMessage();
    this.navigationItems = this.userProfileService.getNavigationItems();
  }

  onLogout() {
    this.authService.logout();
  }
}