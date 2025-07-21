import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, forkJoin } from 'rxjs';
import { VehicleService, CustomerService, ReservationService } from '../../core/services';
import { Vehicle, Customer, CreateCustomer, CreateReservation, ReservationStatus } from '../../shared/models';

@Component({
  selector: 'app-booking-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './booking-form.component.html',
  styleUrls: ['./booking-form.component.css']
})
export class BookingFormComponent implements OnInit {
  vehicle$!: Observable<Vehicle>;
  
  bookingData = {
    vehicleId: 0,
    startDate: new Date(),
    endDate: new Date(),
    totalPrice: 0
  };

  customerData: CreateCustomer = {
    name: '',
    email: '',
    phone: '',
    documentNumber: ''
  };

  isSubmitting = false;
  showSuccessMessage = false;
  errorMessage = '';
  confirmationNumber = '';

  constructor(
    private vehicleService: VehicleService,
    private customerService: CustomerService,
    private reservationService: ReservationService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadBookingData();
  }

  private loadBookingData(): void {
    this.route.queryParams.subscribe(params => {
      this.bookingData.vehicleId = parseInt(params['vehicleId'], 10);
      this.bookingData.startDate = new Date(params['startDate']);
      this.bookingData.endDate = new Date(params['endDate']);
      this.bookingData.totalPrice = parseFloat(params['totalPrice']);

      if (this.bookingData.vehicleId) {
        this.vehicle$ = this.vehicleService.getVehicleById(this.bookingData.vehicleId);
      }
    });
  }

  onSubmit(form: NgForm): void {
    if (!form.valid || this.isSubmitting) {
      return;
    }

    if (!this.validateCustomerData()) {
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    // Create reservation with customer info (backend handles customer creation)
    this.createReservation();
  }

  private createReservation(): void {
    const reservationData: CreateReservation = {
      vehicleId: this.bookingData.vehicleId,
      startDate: this.bookingData.startDate,
      endDate: this.bookingData.endDate,
      customerInfo: {
        name: this.customerData.name,
        email: this.customerData.email,
        phone: this.customerData.phone,
        documentNumber: this.customerData.documentNumber
      }
    };

    this.reservationService.createReservation(reservationData).subscribe({
      next: (reservation) => {
        this.confirmationNumber = reservation.confirmationNumber;
        this.showSuccessMessage = true;
        this.isSubmitting = false;
        
        // Redirect to confirmation page after 3 seconds
        setTimeout(() => {
          this.router.navigate(['/booking/confirmation'], {
            queryParams: { confirmationNumber: this.confirmationNumber }
          });
        }, 3000);
      },
      error: (error) => {
        console.error('Error creating reservation:', error);
        this.errorMessage = 'Error al crear la reserva. Por favor, inténtalo de nuevo.';
        this.isSubmitting = false;
      }
    });
  }

  private validateCustomerData(): boolean {
    if (!this.customerService.validateEmail(this.customerData.email)) {
      this.errorMessage = 'Por favor, ingresa un email válido.';
      return false;
    }


    if (!this.customerService.validateDocumentNumber(this.customerData.documentNumber)) {
      this.errorMessage = 'Por favor, ingresa un número de documento válido.';
      return false;
    }

    // Age validation removed as birthDate is not required by backend

    return true;
  }

  formatPrice(price: number): string {
    return this.vehicleService.formatPrice(price);
  }



  get totalDays(): number {
    const timeDiff = this.bookingData.endDate.getTime() - this.bookingData.startDate.getTime();
    return Math.ceil(timeDiff / (1000 * 3600 * 24));
  }

  get maxBirthDate(): string {
    const date = new Date();
    date.setFullYear(date.getFullYear() - 18);
    return date.toISOString().split('T')[0];
  }

  goBack(): void {
    window.history.back();
  }

  navigateToConfirmation(): void {
    this.router.navigate(['/booking/confirmation'], {
      queryParams: { confirmationNumber: this.confirmationNumber }
    });
  }
}