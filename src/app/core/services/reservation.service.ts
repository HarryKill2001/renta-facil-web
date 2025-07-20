import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Reservation, CreateReservation, ReservationStatus } from '../../shared/models';

@Injectable({
  providedIn: 'root'
})
export class ReservationService {
  constructor(private apiService: ApiService) {}

  createReservation(reservation: CreateReservation): Observable<Reservation> {
    return this.apiService.createReservation(reservation);
  }

  getReservationById(id: number): Observable<Reservation> {
    return this.apiService.getReservation(id);
  }

  getReservationByConfirmationNumber(confirmationNumber: string): Observable<Reservation> {
    return this.apiService.getReservationByConfirmation(confirmationNumber);
  }

  cancelReservation(id: number, reason: string): Observable<Reservation> {
    return this.apiService.cancelReservation(id, reason);
  }

  confirmReservation(id: number): Observable<Reservation> {
    return this.apiService.confirmReservation(id);
  }

  getStatusDisplayName(status: ReservationStatus): string {
    const statusMap = {
      [ReservationStatus.Pending]: 'Pendiente',
      [ReservationStatus.Confirmed]: 'Confirmada',
      [ReservationStatus.Cancelled]: 'Cancelada',
      [ReservationStatus.Completed]: 'Completada'
    };
    return statusMap[status] || status;
  }

  getStatusClass(status: ReservationStatus): string {
    const classMap = {
      [ReservationStatus.Pending]: 'status-pending',
      [ReservationStatus.Confirmed]: 'status-confirmed',
      [ReservationStatus.Cancelled]: 'status-cancelled',
      [ReservationStatus.Completed]: 'status-completed'
    };
    return classMap[status] || '';
  }

  isEditable(status: ReservationStatus): boolean {
    return status === ReservationStatus.Pending;
  }

  isCancellable(status: ReservationStatus): boolean {
    return status === ReservationStatus.Pending || status === ReservationStatus.Confirmed;
  }

  generateConfirmationNumber(): string {
    const date = new Date();
    const dateStr = date.toISOString().split('T')[0].replace(/-/g, '');
    const randomNum = Math.floor(Math.random() * 1000).toString().padStart(3, '0');
    return `RF-${dateStr}-${randomNum}`;
  }
}