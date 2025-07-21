import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  duration?: number;
  dismissible?: boolean;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notifications = new BehaviorSubject<Notification[]>([]);
  private idCounter = 0;

  get notifications$(): Observable<Notification[]> {
    return this.notifications.asObservable();
  }

  showSuccess(title: string, message: string, duration: number = 5000): string {
    return this.addNotification('success', title, message, duration);
  }

  showError(title: string, message: string, duration: number = 0): string {
    return this.addNotification('error', title, message, duration);
  }

  showWarning(title: string, message: string, duration: number = 7000): string {
    return this.addNotification('warning', title, message, duration);
  }

  showInfo(title: string, message: string, duration: number = 5000): string {
    return this.addNotification('info', title, message, duration);
  }

  private addNotification(
    type: Notification['type'],
    title: string,
    message: string,
    duration: number
  ): string {
    const id = `notification-${++this.idCounter}`;
    const notification: Notification = {
      id,
      type,
      title,
      message,
      duration,
      dismissible: true,
      timestamp: new Date()
    };

    const currentNotifications = this.notifications.value;
    this.notifications.next([...currentNotifications, notification]);

    if (duration > 0) {
      setTimeout(() => {
        this.dismissNotification(id);
      }, duration);
    }

    return id;
  }

  dismissNotification(id: string): void {
    const currentNotifications = this.notifications.value;
    const filteredNotifications = currentNotifications.filter(n => n.id !== id);
    this.notifications.next(filteredNotifications);
  }

  clearAll(): void {
    this.notifications.next([]);
  }

  // Convenience methods for common scenarios
  showValidationError(message: string = 'Por favor, verifica los datos ingresados'): string {
    return this.showError('Error de validación', message);
  }

  showNetworkError(message: string = 'Error de conexión. Verifica tu conexión a internet'): string {
    return this.showError('Error de conexión', message);
  }

  showServerError(message: string = 'Error del servidor. Inténtalo de nuevo más tarde'): string {
    return this.showError('Error del servidor', message);
  }

  showReservationSuccess(confirmationNumber: string): string {
    return this.showSuccess(
      'Reserva confirmada',
      `Tu reserva ha sido creada exitosamente. Número de confirmación: ${confirmationNumber}`
    );
  }

  showReservationError(): string {
    return this.showError(
      'Error al crear la reserva',
      'No se pudo procesar tu reserva. Por favor, inténtalo de nuevo.'
    );
  }

  showVehicleUnavailable(): string {
    return this.showWarning(
      'Vehículo no disponible',
      'El vehículo seleccionado no está disponible para las fechas elegidas.'
    );
  }

  showBookingCancelled(confirmationNumber: string): string {
    return this.showInfo(
      'Reserva cancelada',
      `La reserva ${confirmationNumber} ha sido cancelada exitosamente.`
    );
  }

  showFormSaved(): string {
    return this.showSuccess(
      'Información guardada',
      'Tus datos han sido guardados correctamente.'
    );
  }

  showSessionExpired(): string {
    return this.showWarning(
      'Sesión expirada',
      'Tu sesión ha expirado. Por favor, inicia sesión nuevamente.'
    );
  }

  showMaintenanceMode(): string {
    return this.showInfo(
      'Mantenimiento programado',
      'El sistema estará en mantenimiento. Algunas funciones pueden no estar disponibles.'
    );
  }
}