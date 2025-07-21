import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { VehicleService } from '../../core/services';
import { Vehicle, VehicleType, VehicleAvailabilityRequest } from '../../shared/models';
import { VehicleCardSkeletonComponent } from '../../shared/components/vehicle-card-skeleton/vehicle-card-skeleton.component';

@Component({
  selector: 'app-vehicle-search',
  standalone: true,
  imports: [CommonModule, FormsModule, VehicleCardSkeletonComponent],
  templateUrl: './vehicle-search.component.html',
  styleUrls: ['./vehicle-search.component.css']
})
export class VehicleSearchComponent implements OnInit {
  vehicles$!: Observable<Vehicle[]>;
  availabilityRequest: VehicleAvailabilityRequest = {
    startDate: new Date(),
    endDate: new Date(),
    type: undefined
  };
  
  vehicleTypes = [VehicleType.SUV, VehicleType.Sedan, VehicleType.Compact];
  isSearching = false;
  hasSearched = false;
  isLoading = true;

  constructor(
    private vehicleService: VehicleService,
    private router: Router
  ) {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.availabilityRequest.endDate = tomorrow;
  }

  ngOnInit(): void {
    this.loadAllVehicles();
  }

  loadAllVehicles(): void {
    this.isLoading = true;
    this.vehicles$ = this.vehicleService.getAllVehicles();
    
    setTimeout(() => {
      this.isLoading = false;
    }, 1500);
  }

  searchAvailableVehicles(): void {
    if (!this.isValidDateRange()) {
      return;
    }

    this.isSearching = true;
    this.isLoading = true;
    this.hasSearched = true;
    
    this.vehicles$ = this.vehicleService.getAvailableVehicles(this.availabilityRequest);
    
    setTimeout(() => {
      this.isSearching = false;
      this.isLoading = false;
    }, 1000);
  }

  resetSearch(): void {
    this.availabilityRequest = {
      startDate: new Date(),
      endDate: new Date(Date.now() + 24 * 60 * 60 * 1000),
      type: undefined
    };
    this.hasSearched = false;
    this.loadAllVehicles();
  }

  calculateTotalPrice(vehicle: Vehicle): number {
    return this.vehicleService.calculateTotalPrice(
      vehicle,
      this.availabilityRequest.startDate,
      this.availabilityRequest.endDate
    );
  }

  formatPrice(price: number): string {
    return this.vehicleService.formatPrice(price);
  }

  selectVehicle(vehicle: Vehicle): void {
    const bookingData = {
      vehicleId: vehicle.id,
      startDate: this.availabilityRequest.startDate,
      endDate: this.availabilityRequest.endDate,
      totalPrice: this.calculateTotalPrice(vehicle)
    };

    this.router.navigate(['/booking'], { 
      queryParams: bookingData 
    });
  }

  viewDetails(vehicle: Vehicle): void {
    this.router.navigate(['/vehicles', vehicle.id]);
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

  private isValidDateRange(): boolean {
    const start = new Date(this.availabilityRequest.startDate);
    const end = new Date(this.availabilityRequest.endDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return start >= today && end > start;
  }

  get minStartDate(): string {
    return new Date().toISOString().split('T')[0];
  }

  get minEndDate(): string {
    const startDate = new Date(this.availabilityRequest.startDate);
    startDate.setDate(startDate.getDate() + 1);
    return startDate.toISOString().split('T')[0];
  }
}