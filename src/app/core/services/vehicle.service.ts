import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Vehicle, VehicleType, VehicleAvailabilityRequest, CreateVehicle, UpdateVehicle } from '../../shared/models';

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
      request.type as any
    );
  }

  checkAvailability(vehicleId: number, startDate: Date, endDate: Date): Observable<boolean> {
    return this.apiService.checkVehicleAvailability(vehicleId, startDate, endDate);
  }

  createVehicle(vehicle: CreateVehicle): Observable<Vehicle> {
    return this.apiService.createVehicle(vehicle);
  }

  updateVehicle(id: number, vehicle: Partial<CreateVehicle>): Observable<Vehicle> {
    return this.apiService.updateVehicle(id, vehicle);
  }

  deleteVehicle(id: number): Observable<void> {
    return this.apiService.deleteVehicle(id);
  }

  getVehicleTypes(): VehicleType[] {
    return [VehicleType.SUV, VehicleType.Sedan, VehicleType.Compact];
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

  filterVehiclesByType(vehicles: Vehicle[], type: VehicleType): Vehicle[] {
    return vehicles.filter(vehicle => vehicle.type === type);
  }

  filterAvailableVehicles(vehicles: Vehicle[]): Vehicle[] {
    return vehicles.filter(vehicle => vehicle.available);
  }

  filterVehiclesByPriceRange(vehicles: Vehicle[], minPrice: number, maxPrice: number): Vehicle[] {
    return vehicles.filter(vehicle => 
      vehicle.pricePerDay >= minPrice && vehicle.pricePerDay <= maxPrice
    );
  }

  validateVehicleData(vehicle: any): boolean {
    return !!(
      vehicle.model && 
      vehicle.model.trim() !== '' &&
      vehicle.type !== undefined &&
      vehicle.year >= 2000 && 
      vehicle.year <= new Date().getFullYear() + 1 &&
      vehicle.pricePerDay >= 0.01 &&
      vehicle.pricePerDay <= 1000
    );
  }
}