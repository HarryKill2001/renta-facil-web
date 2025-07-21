import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  loginData = {
    email: '',
    password: ''
  };

  isLoading = false;
  errorMessage = '';
  showDemoCredentials = true;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit(form: NgForm): void {
    if (!form.valid || this.isLoading) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginData.email, this.loginData.password).subscribe({
      next: (success) => {
        this.isLoading = false;
        if (success) {
          const user = this.authService.getCurrentUser();
          if (user?.role === 'admin') {
            this.router.navigate(['/admin']);
          } else {
            this.router.navigate(['/']);
          }
        } else {
          this.errorMessage = 'Credenciales incorrectas. Por favor, inténtalo de nuevo.';
        }
      },
      error: (error) => {
        console.error('Login error:', error);
        this.errorMessage = 'Error al iniciar sesión. Por favor, inténtalo de nuevo.';
        this.isLoading = false;
      }
    });
  }

  useDemoCredentials(type: 'admin' | 'user'): void {
    if (type === 'admin') {
      this.loginData.email = 'admin@rentafacil.com';
      this.loginData.password = 'admin123';
    } else {
      this.loginData.email = 'user@rentafacil.com';
      this.loginData.password = 'user123';
    }
  }

  goBack(): void {
    this.router.navigate(['/']);
  }
}