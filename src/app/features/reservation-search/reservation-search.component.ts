import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ReservationService } from '../../core/services';
import { Reservation } from '../../shared/models';

@Component({
  selector: 'app-reservation-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reservation-search.component.html',
  styleUrls: ['./reservation-search.component.css']
})
export class ReservationSearchComponent implements OnInit {
  searchData = {
    confirmationNumber: '',
    email: ''
  };

  searchResults: Reservation[] = [];
  isSearching = false;
  hasSearched = false;
  errorMessage = '';

  constructor(
    private reservationService: ReservationService,
    private router: Router
  ) {}

  ngOnInit(): void {}

  onSearch(form: NgForm): void {
    if (!form.valid || this.isSearching) {
      return;
    }

    if (!this.searchData.confirmationNumber && !this.searchData.email) {
      this.errorMessage = 'Ingresa al menos el número de confirmación o el correo electrónico.';
      return;
    }

    this.isSearching = true;
    this.errorMessage = '';
    this.hasSearched = false;

    // Search by confirmation number first if provided
    if (this.searchData.confirmationNumber) {
      this.reservationService.getReservationByConfirmationNumber(this.searchData.confirmationNumber).subscribe({
        next: (reservation) => {
          this.searchResults = reservation ? [reservation] : [];
          this.hasSearched = true;
          this.isSearching = false;
        },
        error: (error) => {
          console.error('Error searching by confirmation number:', error);
          this.errorMessage = 'No se encontró ninguna reserva con ese número de confirmación.';
          this.isSearching = false;
          this.hasSearched = true;
          this.searchResults = [];
        }
      });
    } else {
      // If only email is provided, use general search (this will need backend support)
      this.reservationService.searchReservations({}).subscribe({
        next: (reservations) => {
          this.searchResults = reservations;
          this.hasSearched = true;
          this.isSearching = false;
        },
        error: (error) => {
          console.error('Error searching reservations:', error);
          this.errorMessage = 'Error al buscar reservas. Por favor, inténtalo de nuevo.';
          this.isSearching = false;
          this.hasSearched = true;
          this.searchResults = [];
        }
      });
    }
  }

  viewReservation(reservation: Reservation): void {
    this.router.navigate(['/booking/confirmation'], {
      queryParams: { confirmationNumber: reservation.confirmationNumber }
    });
  }

  getStatusDisplayName(status: string): string {
    const statusMap: { [key: string]: string } = {
      'Pending': 'Pendiente',
      'Confirmed': 'Confirmada',
      'Cancelled': 'Cancelada',
      'Completed': 'Completada'
    };
    return statusMap[status] || status;
  }

  getStatusClass(status: string): string {
    const statusClasses: { [key: string]: string } = {
      'Pending': 'status-pending',
      'Confirmed': 'status-confirmed',
      'Cancelled': 'status-cancelled',
      'Completed': 'status-completed'
    };
    return statusClasses[status] || 'status-default';
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0
    }).format(price);
  }

  clearSearch(): void {
    this.searchData = {
      confirmationNumber: '',
      email: ''
    };
    this.searchResults = [];
    this.hasSearched = false;
    this.errorMessage = '';
  }
}