import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class BrowserStorageService {

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {}

  private isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  getItem(key: string, storage: 'local' | 'session' = 'local'): string | null {
    if (!this.isBrowser()) return null;
    
    try {
      const storageObj = storage === 'local' ? localStorage : sessionStorage;
      return storageObj.getItem(key);
    } catch {
      return null;
    }
  }

  setItem(key: string, value: string, storage: 'local' | 'session' = 'local'): void {
    if (!this.isBrowser()) return;
    
    try {
      const storageObj = storage === 'local' ? localStorage : sessionStorage;
      storageObj.setItem(key, value);
    } catch {
      // Silently fail if storage is not available
    }
  }

  removeItem(key: string, storage: 'local' | 'session' = 'local'): void {
    if (!this.isBrowser()) return;
    
    try {
      const storageObj = storage === 'local' ? localStorage : sessionStorage;
      storageObj.removeItem(key);
    } catch {
      // Silently fail if storage is not available
    }
  }

  clear(storage: 'local' | 'session' = 'local'): void {
    if (!this.isBrowser()) return;
    
    try {
      const storageObj = storage === 'local' ? localStorage : sessionStorage;
      storageObj.clear();
    } catch {
      // Silently fail if storage is not available
    }
  }
}