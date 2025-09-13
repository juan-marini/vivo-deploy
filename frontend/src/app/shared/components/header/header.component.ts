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
    <header class="bg-white shadow-lg">
      <div class="container-fluid">
        <div class="d-flex justify-content-between align-items-center py-3 px-4">
          <!-- Welcome Message na esquerda -->
          <div class="d-flex align-items-center">
            <div class="vivo-logo me-3">
              <span class="text-white fw-bold">VIVO</span>
            </div>
            <div class="welcome-section">
              <h1 class="h4 mb-0 text-dark fw-bold">{{ welcomeMessage }}</h1>
              <p class="small text-muted mb-0">Sistema de Onboarding</p>
            </div>
          </div>

          <!-- Navigation na direita -->
          <nav class="d-flex align-items-center">
            <a
              *ngFor="let item of navigationItems"
              [routerLink]="item.route"
              class="nav-link text-muted fw-medium me-4"
              routerLinkActive="text-purple active">
              <i [class]="item.icon + ' me-1'"></i>{{ item.label }}
            </a>

            <button class="btn btn-outline-purple" (click)="onLogout()">
              <i class="fas fa-sign-out-alt me-1"></i>Sair
            </button>
          </nav>
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