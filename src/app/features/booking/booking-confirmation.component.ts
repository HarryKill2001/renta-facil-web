import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, forkJoin } from 'rxjs';
import { VehicleService, CustomerService, ReservationService } from '../../core/services';
import { Reservation, Vehicle, Customer, ReservationStatus } from '../../shared/models';

@Component({
  selector: 'app-booking-confirmation',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './booking-confirmation.component.html',
  styleUrls: ['./booking-confirmation.component.css']
})
export class BookingConfirmationComponent implements OnInit {
  reservation$!: Observable<Reservation>;
  confirmationNumber = '';
  isLoading = true;
  errorMessage = '';

  constructor(
    private vehicleService: VehicleService,
    private customerService: CustomerService,
    private reservationService: ReservationService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.confirmationNumber = params['confirmationNumber'];
      if (this.confirmationNumber) {
        this.loadReservationDetails();
      } else {
        this.errorMessage = 'Número de confirmación no válido.';
        this.isLoading = false;
      }
    });
  }

  private loadReservationDetails(): void {
    this.reservationService.getReservationByConfirmationNumber(this.confirmationNumber).subscribe({
      next: (reservation) => {
        this.reservation$ = new Observable(observer => {
          observer.next(reservation);
          observer.complete();
        });
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading reservation:', error);
        this.errorMessage = 'No se pudo cargar la información de la reserva.';
        this.isLoading = false;
      }
    });
  }

  cancelReservation(reservationId: number): void {
    if (!confirm('¿Estás seguro de que quieres cancelar esta reserva?')) {
      return;
    }

    const reason = prompt('Por favor, indica el motivo de la cancelación:');
    if (!reason) {
      return;
    }

    this.reservationService.cancelReservation(reservationId, reason).subscribe({
      next: (updatedReservation) => {
        this.reservation$ = new Observable(observer => {
          observer.next(updatedReservation);
          observer.complete();
        });
        alert('Reserva cancelada exitosamente.');
      },
      error: (error) => {
        console.error('Error canceling reservation:', error);
        alert('Error al cancelar la reserva. Por favor, inténtalo de nuevo.');
      }
    });
  }

  printConfirmation(): void {
    window.print();
  }

  downloadPDF(): void {
    // TODO: Implement PDF generation
    alert('Funcionalidad de descarga PDF próximamente disponible.');
  }

  goToVehicles(): void {
    this.router.navigate(['/vehicles']);
  }

  formatPrice(price: number): string {
    return this.vehicleService.formatPrice(price);
  }

  getStatusDisplayName(status: ReservationStatus): string {
    return this.reservationService.getStatusDisplayName(status);
  }

  getStatusClass(status: ReservationStatus): string {
    return this.reservationService.getStatusClass(status);
  }

  isCancellable(status: ReservationStatus): boolean {
    return this.reservationService.isCancellable(status);
  }

  get totalDays(): number {
    // This would need to be calculated from the reservation dates
    return 1; // Placeholder
  }
}