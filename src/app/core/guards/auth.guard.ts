import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private router: Router) {}

  canActivate(): Observable<boolean> | Promise<boolean> | boolean {
    // For now, we'll simulate authentication check
    // In a real app, this would check with an auth service
    const isAuthenticated = this.checkAuthentication();
    
    if (!isAuthenticated) {
      // Redirect to login page (for now, redirect to home)
      this.router.navigate(['/']);
      return false;
    }
    
    return true;
  }

  private checkAuthentication(): boolean {
    // Simulate authentication check
    // In a real app, you'd check JWT token, session, etc.
    
    // For demo purposes, check if user has admin access in localStorage
    const userRole = localStorage.getItem('userRole');
    return userRole === 'admin';
  }
}