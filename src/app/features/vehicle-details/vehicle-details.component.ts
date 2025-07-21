import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, switchMap } from 'rxjs';
import { VehicleService } from '../../core/services';
import { Vehicle, VehicleType } from '../../shared/models';

@Component({
  selector: 'app-vehicle-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './vehicle-details.component.html',
  styleUrls: ['./vehicle-details.component.css']
})
export class VehicleDetailsComponent implements OnInit {
  @Input() vehicleId?: number;
  
  vehicle$!: Observable<Vehicle>;
  selectedStartDate: Date = new Date();
  selectedEndDate: Date = new Date(Date.now() + 24 * 60 * 60 * 1000);
  isCheckingAvailability = false;
  availabilityResult: boolean | null = null;

  constructor(
    private vehicleService: VehicleService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadVehicleDetails();
  }

  private loadVehicleDetails(): void {
    if (this.vehicleId) {
      this.vehicle$ = this.vehicleService.getVehicleById(this.vehicleId);
    } else {
      this.vehicle$ = this.route.params.pipe(
        switchMap(params => {
          const id = parseInt(params['id'], 10);
          return this.vehicleService.getVehicleById(id);
        })
      );
    }
  }

  updateStartDate(event: any): void {
    this.selectedStartDate = new Date(event.target.value);
  }

  updateEndDate(event: any): void {
    this.selectedEndDate = new Date(event.target.value);
  }

  checkAvailability(vehicleId: number): void {
    if (!this.isDateRangeValid) {
      return;
    }

    this.isCheckingAvailability = true;
    this.availabilityResult = null;

    this.vehicleService.checkAvailability(
      vehicleId,
      this.selectedStartDate,
      this.selectedEndDate
    ).subscribe({
      next: (available) => {
        this.availabilityResult = available;
        this.isCheckingAvailability = false;
      },
      error: (error) => {
        console.error('Error checking availability:', error);
        this.isCheckingAvailability = false;
      }
    });
  }

  bookVehicle(vehicle: Vehicle): void {
    if (!this.isDateRangeValid) {
      return;
    }

    const bookingData = {
      vehicleId: vehicle.id,
      startDate: this.selectedStartDate,
      endDate: this.selectedEndDate,
      totalPrice: this.calculateTotalPrice(vehicle)
    };

    // TODO: Navigate to booking form with vehicle and date data
    console.log('Booking vehicle:', bookingData);
    this.router.navigate(['/booking'], { 
      queryParams: bookingData 
    });
  }

  calculateTotalPrice(vehicle: Vehicle): number {
    return this.vehicleService.calculateTotalPrice(
      vehicle,
      this.selectedStartDate,
      this.selectedEndDate
    );
  }

  formatPrice(price: number): string {
    return this.vehicleService.formatPrice(price);
  }

  getVehicleTypeDisplay(type: VehicleType): string {
    switch (type) {
      case VehicleType.SUV:
        return 'SUV';
      case VehicleType.Sedan:
        return 'Sedán';
      case VehicleType.Compact:
        return 'Compacto';
      default:
        return 'Desconocido';
    }
  }

  goBack(): void {
    window.history.back();
  }

  get isDateRangeValid(): boolean {
    const start = new Date(this.selectedStartDate);
    const end = new Date(this.selectedEndDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return start >= today && end > start;
  }

  get minStartDate(): string {
    return new Date().toISOString().split('T')[0];
  }

  get minEndDate(): string {
    const startDate = new Date(this.selectedStartDate);
    startDate.setDate(startDate.getDate() + 1);
    return startDate.toISOString().split('T')[0];
  }

  get totalDays(): number {
    const timeDiff = this.selectedEndDate.getTime() - this.selectedStartDate.getTime();
    return Math.ceil(timeDiff / (1000 * 3600 * 24));
  }
}