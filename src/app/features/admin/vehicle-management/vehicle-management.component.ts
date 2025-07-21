import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { VehicleService } from '../../../core/services';
import { Vehicle, VehicleType } from '../../../shared/models';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';

@Component({
  selector: 'app-vehicle-management',
  standalone: true,
  imports: [CommonModule, RouterModule, SkeletonComponent],
  templateUrl: './vehicle-management.component.html',
  styleUrls: ['./vehicle-management.component.css']
})
export class VehicleManagementComponent implements OnInit {
  vehicles: Vehicle[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(
    private vehicleService: VehicleService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadVehicles();
  }

  loadVehicles(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.vehicleService.getAllVehicles().subscribe({
      next: (vehicles) => {
        this.vehicles = vehicles;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading vehicles:', error);
        this.errorMessage = 'Error al cargar los vehículos';
        this.isLoading = false;
      }
    });
  }

  createNewVehicle(): void {
    this.router.navigate(['/admin/vehicles/new']);
  }

  editVehicle(vehicleId: number): void {
    this.router.navigate(['/admin/vehicles', vehicleId, 'edit']);
  }

  deleteVehicle(vehicleId: number): void {
    if (confirm('¿Estás seguro de que quieres eliminar este vehículo?')) {
      // TODO: Implement delete functionality when backend is ready
      console.log('Delete vehicle:', vehicleId);
    }
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0
    }).format(price);
  }

  getStatusClass(available: boolean): string {
    return available ? 'status-available' : 'status-unavailable';
  }

  getStatusText(available: boolean): string {
    return available ? 'Disponible' : 'No disponible';
  }

  getVehicleTypeDisplay(type: VehicleType): string {
    switch (type) {
      case VehicleType.SUV:
        return 'SUV';
      case VehicleType.Sedan:
        return 'Sedan';
      case VehicleType.Compact:
        return 'Compact';
      default:
        return 'Desconocido';
    }
  }

  goBack(): void {
    this.router.navigate(['/admin']);
  }
}