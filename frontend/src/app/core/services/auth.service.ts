import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../../enviroments/enviroment';
import { LoginRequest, LoginResponse, Usuario } from '../models/auth.model';
import { BrowserStorageService } from './browser-storage.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private currentUserSubject: BehaviorSubject<Usuario | null>;
  public currentUser: Observable<Usuario | null>;
  private tokenKey = 'vivo_token';
  private refreshTokenKey = 'vivo_refresh_token';
  private userKey = 'vivo_user';

  constructor(
    private http: HttpClient,
    private router: Router,
    private storage: BrowserStorageService
  ) {
    const storedUser = this.getStoredUser();
    this.currentUserSubject = new BehaviorSubject<Usuario | null>(storedUser);
    this.currentUser = this.currentUserSubject.asObservable();
  }


  public get currentUserValue(): Usuario | null {
    return this.currentUserSubject.value;
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap(response => {
          if (response.success && response.token) {
            this.storeTokens(response.token, response.refreshToken);
            this.storeUser(response.usuario!);
            this.currentUserSubject.next(response.usuario!);
          }
        })
      );
  }

  logout(): void {
    const token = this.getToken();
    if (token) {
      this.http.post(`${this.apiUrl}/logout`, {}).subscribe({
        error: () => {}
      });
    }

    this.clearStorage();
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return of({ success: false, message: 'No refresh token' });
    }

    return this.http.post<LoginResponse>(`${this.apiUrl}/refresh-token`, { refreshToken })
      .pipe(
        tap(response => {
          if (response.success && response.token) {
            this.storeTokens(response.token, response.refreshToken);
          }
        }),
        catchError(() => {
          this.logout();
          return of({ success: false, message: 'Token refresh failed' });
        })
      );
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/esqueci-senha`, { email });
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = this.parseJwt(token);
      if (!payload) return false;

      const currentTime = Date.now() / 1000;
      return payload.exp > currentTime;
    } catch {
      return false;
    }
  }

  getToken(): string | null {
    return this.storage.getItem(this.tokenKey, 'session') || this.storage.getItem(this.tokenKey, 'local');
  }

  private getRefreshToken(): string | null {
    return this.storage.getItem(this.refreshTokenKey, 'session') || this.storage.getItem(this.refreshTokenKey, 'local');
  }

  private storeTokens(token: string, refreshToken?: string): void {
    const storageType = this.storage.getItem('rememberedEmail', 'local') ? 'local' : 'session';
    this.storage.setItem(this.tokenKey, token, storageType);
    if (refreshToken) {
      this.storage.setItem(this.refreshTokenKey, refreshToken, storageType);
    }
  }

  private storeUser(user: Usuario): void {
    const storageType = this.storage.getItem('rememberedEmail', 'local') ? 'local' : 'session';
    this.storage.setItem(this.userKey, JSON.stringify(user), storageType);
  }

  private getStoredUser(): Usuario | null {
    const userJson = this.storage.getItem(this.userKey, 'session') || this.storage.getItem(this.userKey, 'local');
    if (userJson) {
      try {
        return JSON.parse(userJson);
      } catch {
        return null;
      }
    }
    return null;
  }

  private clearStorage(): void {
    this.storage.removeItem(this.tokenKey, 'session');
    this.storage.removeItem(this.refreshTokenKey, 'session');
    this.storage.removeItem(this.userKey, 'session');

    const rememberedEmail = this.storage.getItem('rememberedEmail', 'local');
    this.storage.removeItem(this.tokenKey, 'local');
    this.storage.removeItem(this.refreshTokenKey, 'local');
    this.storage.removeItem(this.userKey, 'local');

    if (rememberedEmail) {
      this.storage.setItem('rememberedEmail', rememberedEmail, 'local');
    }
  }

  private parseJwt(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch {
      return null;
    }
  }
}
