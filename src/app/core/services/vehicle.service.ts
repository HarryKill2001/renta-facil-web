import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Vehicle, VehicleType, VehicleAvailabilityRequest } from '../../shared/models';

@Injectable({
  providedIn: 'root'
})
export class VehicleService {
  constructor(private apiService: ApiService) {}

  getAllVehicles(): Observable<Vehicle[]> {
    return this.apiService.getVehicles();
  }

  getVehicleById(id: number): Observable<Vehicle> {
    return this.apiService.getVehicle(id);
  }

  getAvailableVehicles(request: VehicleAvailabilityRequest): Observable<Vehicle[]> {
    return this.apiService.getAvailableVehicles(
      request.startDate,
      request.endDate,
      request.type
    );
  }

  checkAvailability(vehicleId: number, startDate: Date, endDate: Date): Observable<boolean> {
    return this.apiService.checkVehicleAvailability(vehicleId, startDate, endDate);
  }

  getVehicleTypes(): VehicleType[] {
    return Object.values(VehicleType);
  }

  calculateTotalPrice(vehicle: Vehicle, startDate: Date, endDate: Date): number {
    const timeDiff = endDate.getTime() - startDate.getTime();
    const daysDiff = Math.ceil(timeDiff / (1000 * 3600 * 24));
    return daysDiff * vehicle.pricePerDay;
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP'
    }).format(price);
  }
}