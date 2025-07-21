import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { BehaviorSubject, Observable } from 'rxjs';

export interface User {
  id: number;
  email: string;
  name: string;
  role: 'admin' | 'user';
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {
    // Check if user is already logged in (only in browser)
    if (isPlatformBrowser(this.platformId)) {
      this.checkExistingSession();
    }
  }

  private checkExistingSession(): void {
    const isAuthenticated = localStorage.getItem('isAuthenticated') === 'true';
    const userRole = localStorage.getItem('userRole');
    const userEmail = localStorage.getItem('userEmail');
    const userName = localStorage.getItem('userName');

    if (isAuthenticated && userRole && userEmail && userName) {
      const user: User = {
        id: 1,
        email: userEmail,
        name: userName,
        role: userRole as 'admin' | 'user'
      };
      this.currentUserSubject.next(user);
    }
  }

  login(email: string, password: string): Observable<boolean> {
    return new Observable(observer => {
      // Simulate API call
      setTimeout(() => {
        // Demo credentials
        if (email === 'admin@rentafacil.com' && password === 'admin123') {
          const user: User = {
            id: 1,
            email: 'admin@rentafacil.com',
            name: 'Administrador',
            role: 'admin'
          };
          
          this.setSession(user);
          observer.next(true);
        } else if (email === 'user@rentafacil.com' && password === 'user123') {
          const user: User = {
            id: 2,
            email: 'user@rentafacil.com',
            name: 'Usuario Demo',
            role: 'user'
          };
          
          this.setSession(user);
          observer.next(true);
        } else {
          observer.next(false);
        }
        observer.complete();
      }, 1000); // Simulate network delay
    });
  }

  logout(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('isAuthenticated');
      localStorage.removeItem('userRole');
      localStorage.removeItem('userEmail');
      localStorage.removeItem('userName');
    }
    this.currentUserSubject.next(null);
  }

  private setSession(user: User): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('isAuthenticated', 'true');
      localStorage.setItem('userRole', user.role);
      localStorage.setItem('userEmail', user.email);
      localStorage.setItem('userName', user.name);
    }
    this.currentUserSubject.next(user);
  }

  isAuthenticated(): boolean {
    if (!isPlatformBrowser(this.platformId)) {
      return false;
    }
    return localStorage.getItem('isAuthenticated') === 'true';
  }

  isAdmin(): boolean {
    if (!isPlatformBrowser(this.platformId)) {
      return false;
    }
    return this.isAuthenticated() && localStorage.getItem('userRole') === 'admin';
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  // Demo method to quickly set admin access for testing
  setAdminAccess(): void {
    if (isPlatformBrowser(this.platformId)) {
      const adminUser: User = {
        id: 1,
        email: 'admin@rentafacil.com',
        name: 'Administrador Demo',
        role: 'admin'
      };
      this.setSession(adminUser);
    }
  }
}