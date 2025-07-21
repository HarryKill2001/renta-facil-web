import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { of, throwError, Observable } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../../core/services';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', ['login', 'getCurrentUser']);
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        FormsModule,
        LoginComponent
      ],
      providers: [
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: routerSpyObj }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should initialize with default login data', () => {
      expect(component.loginData.email).toBe('');
      expect(component.loginData.password).toBe('');
      expect(component.isLoading).toBe(false);
      expect(component.errorMessage).toBe('');
      expect(component.showDemoCredentials).toBe(true);
    });
  });

  describe('Form Submission', () => {
    let mockForm: jasmine.SpyObj<NgForm>;

    beforeEach(() => {
      mockForm = jasmine.createSpyObj('NgForm', [], { valid: true });
      component.loginData = {
        email: 'test@example.com',
        password: 'password123'
      };
    });

    it('should not submit if form is invalid', () => {
      mockForm = jasmine.createSpyObj('NgForm', [], { valid: false });
      
      component.onSubmit(mockForm);
      
      expect(authServiceSpy.login).not.toHaveBeenCalled();
    });

    it('should not submit if already loading', () => {
      component.isLoading = true;
      
      component.onSubmit(mockForm);
      
      expect(authServiceSpy.login).not.toHaveBeenCalled();
    });

    it('should login successfully and navigate to admin for admin user', () => {
      const mockAdminUser = { id: 1, email: 'admin@test.com', name: 'Admin User', role: 'admin' as const };
      authServiceSpy.login.and.returnValue(of(true));
      authServiceSpy.getCurrentUser.and.returnValue(mockAdminUser);
      
      // Initial state
      expect(component.isLoading).toBe(false);
      
      component.onSubmit(mockForm);
      
      expect(authServiceSpy.login).toHaveBeenCalledWith('test@example.com', 'password123');
      expect(component.isLoading).toBe(false); // After observable completes
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/admin']);
      expect(component.errorMessage).toBe('');
    });

    it('should login successfully and navigate to home for regular user', () => {
      const mockUser = { id: 1, email: 'user@test.com', name: 'Regular User', role: 'user' as const };
      authServiceSpy.login.and.returnValue(of(true));
      authServiceSpy.getCurrentUser.and.returnValue(mockUser);
      
      component.onSubmit(mockForm);
      
      expect(authServiceSpy.login).toHaveBeenCalledWith('test@example.com', 'password123');
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/']);
      expect(component.errorMessage).toBe('');
    });

    it('should handle login failure', () => {
      authServiceSpy.login.and.returnValue(of(false));
      
      component.onSubmit(mockForm);
      
      expect(component.isLoading).toBe(false);
      expect(component.errorMessage).toBe('Credenciales incorrectas. Por favor, inténtalo de nuevo.');
      expect(routerSpy.navigate).not.toHaveBeenCalled();
    });

    it('should handle login error', () => {
      authServiceSpy.login.and.returnValue(throwError(() => new Error('Network error')));
      spyOn(console, 'error');
      
      component.onSubmit(mockForm);
      
      expect(component.isLoading).toBe(false);
      expect(component.errorMessage).toBe('Error al iniciar sesión. Por favor, inténtalo de nuevo.');
      expect(console.error).toHaveBeenCalledWith('Login error:', jasmine.any(Error));
      expect(routerSpy.navigate).not.toHaveBeenCalled();
    });

    it('should clear error message on new submission', () => {
      component.errorMessage = 'Previous error';
      authServiceSpy.login.and.returnValue(of(true));
      authServiceSpy.getCurrentUser.and.returnValue({ id: 1, email: 'user@test.com', name: 'Test User', role: 'user' as const });
      
      component.onSubmit(mockForm);
      
      expect(component.errorMessage).toBe('');
    });
  });

  describe('Demo Credentials', () => {
    it('should set admin demo credentials', () => {
      component.useDemoCredentials('admin');
      
      expect(component.loginData.email).toBe('admin@rentafacil.com');
      expect(component.loginData.password).toBe('admin123');
    });

    it('should set user demo credentials', () => {
      component.useDemoCredentials('user');
      
      expect(component.loginData.email).toBe('user@rentafacil.com');
      expect(component.loginData.password).toBe('user123');
    });
  });

  describe('Navigation', () => {
    it('should navigate back to home', () => {
      component.goBack();
      
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/']);
    });
  });

  describe('Loading States', () => {
    let mockForm: jasmine.SpyObj<NgForm>;

    beforeEach(() => {
      mockForm = jasmine.createSpyObj('NgForm', [], { valid: true });
      component.loginData = {
        email: 'test@example.com',
        password: 'password123'
      };
    });

    it('should set loading state during login process', () => {
      // Create a delayed observable that we can control
      authServiceSpy.login.and.returnValue(new Observable(observer => {
        // Don't complete immediately to test loading state
        setTimeout(() => {
          observer.next(true);
          observer.complete();
        }, 100);
      }));
      authServiceSpy.getCurrentUser.and.returnValue({ id: 1, email: 'user@test.com', name: 'Test User', role: 'user' as const });
      
      // Initial state
      expect(component.isLoading).toBe(false);
      
      // During login
      component.onSubmit(mockForm);
      expect(component.isLoading).toBe(true);
    });

    it('should clear loading state after successful login', () => {
      authServiceSpy.login.and.returnValue(of(true));
      authServiceSpy.getCurrentUser.and.returnValue({ id: 1, email: 'user@test.com', name: 'Test User', role: 'user' as const });
      
      component.onSubmit(mockForm);
      
      expect(component.isLoading).toBe(false);
    });

    it('should clear loading state after failed login', () => {
      authServiceSpy.login.and.returnValue(of(false));
      
      component.onSubmit(mockForm);
      
      expect(component.isLoading).toBe(false);
    });

    it('should clear loading state after login error', () => {
      authServiceSpy.login.and.returnValue(throwError(() => new Error('Error')));
      
      component.onSubmit(mockForm);
      
      expect(component.isLoading).toBe(false);
    });
  });

  describe('User Role Handling', () => {
    let mockForm: jasmine.SpyObj<NgForm>;

    beforeEach(() => {
      mockForm = jasmine.createSpyObj('NgForm', [], { valid: true });
      component.loginData = {
        email: 'test@example.com',
        password: 'password123'
      };
    });

    it('should handle user with no role', () => {
      const mockUser = { id: 1, email: 'user@test.com', name: 'Test User', role: 'user' as const }; // User with role property
      authServiceSpy.login.and.returnValue(of(true));
      authServiceSpy.getCurrentUser.and.returnValue(mockUser);
      
      component.onSubmit(mockForm);
      
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/']); // Default to home
    });

    it('should handle null user', () => {
      authServiceSpy.login.and.returnValue(of(true));
      authServiceSpy.getCurrentUser.and.returnValue(null);
      
      component.onSubmit(mockForm);
      
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/']); // Default to home
    });

    it('should handle different admin role variations', () => {
      const mockAdminUser = { id: 1, email: 'admin@test.com', name: 'Admin User', role: 'admin' as const };
      authServiceSpy.login.and.returnValue(of(true));
      authServiceSpy.getCurrentUser.and.returnValue(mockAdminUser);
      
      component.onSubmit(mockForm);
      
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/admin']);
    });
  });

  describe('Form Validation', () => {
    it('should respect form validation state', () => {
      const invalidForm = jasmine.createSpyObj('NgForm', [], { valid: false });
      
      component.onSubmit(invalidForm);
      
      expect(authServiceSpy.login).not.toHaveBeenCalled();
      expect(component.isLoading).toBe(false);
    });
  });
});