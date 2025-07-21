import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  registrationData = {
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    phone: '',
    documentType: 'CC',
    documentNumber: '',
    acceptTerms: false
  };

  isLoading = false;
  errorMessage = '';

  documentTypes = [
    { value: 'CC', label: 'Cédula de Ciudadanía' },
    { value: 'CE', label: 'Cédula de Extranjería' },
    { value: 'PA', label: 'Pasaporte' }
  ];

  constructor(private router: Router) {}

  onSubmit(form: NgForm): void {
    if (!form.valid || this.isLoading) {
      return;
    }

    if (this.registrationData.password !== this.registrationData.confirmPassword) {
      this.errorMessage = 'Las contraseñas no coinciden';
      return;
    }

    if (!this.registrationData.acceptTerms) {
      this.errorMessage = 'Debes aceptar los términos y condiciones';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    // Simulate registration process
    setTimeout(() => {
      // In a real app, this would call an API
      console.log('Registration data:', this.registrationData);
      
      // Show success message and redirect to login
      alert('¡Registro exitoso! Ahora puedes iniciar sesión.');
      this.router.navigate(['/login']);
      
      this.isLoading = false;
    }, 1500);
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  goBack(): void {
    this.router.navigate(['/']);
  }
}