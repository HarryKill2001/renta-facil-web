import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';

export interface ErrorInfo {
  code: string;
  title: string;
  message: string;
  icon: string;
  suggestions: string[];
}

@Component({
  selector: 'app-error-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './error-page.component.html',
  styleUrls: ['./error-page.component.css']
})
export class ErrorPageComponent implements OnInit {
  errorInfo: ErrorInfo = {
    code: '500',
    title: 'Error del Servidor',
    message: 'Algo salió mal en nuestros servidores. Estamos trabajando para solucionarlo.',
    icon: '⚠️',
    suggestions: [
      'Intenta recargar la página',
      'Verifica tu conexión a internet',
      'Espera unos minutos e intenta de nuevo',
      'Contacta nuestro soporte si el problema persiste'
    ]
  };

  private errorTypes: { [key: string]: ErrorInfo } = {
    '400': {
      code: '400',
      title: 'Solicitud Incorrecta',
      message: 'La solicitud no pudo ser procesada debido a información incorrecta.',
      icon: '❌',
      suggestions: [
        'Verifica que todos los campos estén completos',
        'Revisa el formato de los datos ingresados',
        'Intenta de nuevo con información válida',
        'Contacta soporte si necesitas ayuda'
      ]
    },
    '401': {
      code: '401',
      title: 'No Autorizado',
      message: 'Necesitas iniciar sesión para acceder a esta página.',
      icon: '🔒',
      suggestions: [
        'Inicia sesión con tu cuenta',
        'Verifica tus credenciales',
        'Registra una nueva cuenta si no tienes una',
        'Recupera tu contraseña si la olvidaste'
      ]
    },
    '403': {
      code: '403',
      title: 'Acceso Prohibido',
      message: 'No tienes permisos para acceder a esta página.',
      icon: '🚫',
      suggestions: [
        'Verifica que tengas los permisos necesarios',
        'Contacta al administrador',
        'Inicia sesión con una cuenta autorizada',
        'Vuelve a la página principal'
      ]
    },
    '500': {
      code: '500',
      title: 'Error del Servidor',
      message: 'Algo salió mal en nuestros servidores. Estamos trabajando para solucionarlo.',
      icon: '⚠️',
      suggestions: [
        'Intenta recargar la página',
        'Verifica tu conexión a internet',
        'Espera unos minutos e intenta de nuevo',
        'Contacta nuestro soporte si el problema persiste'
      ]
    },
    'network': {
      code: 'NET',
      title: 'Error de Conexión',
      message: 'No se pudo conectar con nuestros servidores. Verifica tu conexión a internet.',
      icon: '📡',
      suggestions: [
        'Verifica tu conexión a internet',
        'Intenta recargar la página',
        'Comprueba que no haya problemas con tu red',
        'Intenta de nuevo en unos minutos'
      ]
    },
    'timeout': {
      code: 'TIME',
      title: 'Tiempo Agotado',
      message: 'La solicitud tardó demasiado tiempo en procesarse.',
      icon: '⏱️',
      suggestions: [
        'Intenta de nuevo',
        'Verifica tu conexión a internet',
        'Espera unos minutos antes de reintentar',
        'Contacta soporte si el problema persiste'
      ]
    }
  };

  constructor(
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const errorType = params['type'] || '500';
      this.errorInfo = this.errorTypes[errorType] || this.errorTypes['500'];
    });
  }

  goHome(): void {
    this.router.navigate(['/']);
  }

  goBack(): void {
    window.history.back();
  }

  retryAction(): void {
    window.location.reload();
  }

  contactSupport(): void {
    this.router.navigate(['/contact']);
  }

  generateErrorId(): string {
    const timestamp = Date.now();
    const random = Math.floor(Math.random() * 1000).toString().padStart(3, '0');
    return `ERR-${timestamp}-${random}`;
  }

  getCurrentTime(): string {
    return new Date().toLocaleString('es-CO', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  }
}