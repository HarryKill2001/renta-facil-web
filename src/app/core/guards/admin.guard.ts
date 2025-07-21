import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  canActivate(): Observable<boolean> | Promise<boolean> | boolean {
    const hasAdminAccess = this.checkAdminAccess();
    
    if (!hasAdminAccess) {
      // During SSR, allow access (will be protected on client-side)
      if (!isPlatformBrowser(this.platformId)) {
        return true;
      }
      
      // Redirect to login page if not authenticated
      const isAuthenticated = localStorage.getItem('isAuthenticated') === 'true';
      if (!isAuthenticated) {
        this.router.navigate(['/login']);
      } else {
        // User is authenticated but not admin - redirect to home
        this.router.navigate(['/']);
      }
      return false;
    }
    
    return true;
  }

  private checkAdminAccess(): boolean {
    if (!isPlatformBrowser(this.platformId)) {
      return false;
    }
    
    // Check if user has admin role
    const userRole = localStorage.getItem('userRole');
    const isAuthenticated = localStorage.getItem('isAuthenticated') === 'true';
    
    return isAuthenticated && userRole === 'admin';
  }
}