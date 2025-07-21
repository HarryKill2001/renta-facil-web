import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { VehicleService } from '../../../core/services';
import { Vehicle, CreateVehicle, VehicleType } from '../../../shared/models';

@Component({
  selector: 'app-vehicle-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vehicle-form.component.html',
  styleUrls: ['./vehicle-form.component.css']
})
export class VehicleFormComponent implements OnInit {
  vehicleData: CreateVehicle = {
    model: '',
    type: VehicleType.Sedan,
    year: null as any,
    pricePerDay: null as any
  };

  isEditMode = false;
  vehicleId: number | null = null;
  isSubmitting = false;
  errorMessage = '';
  isLoading = false;

  vehicleTypes = [
    { value: VehicleType.SUV, label: 'SUV' },
    { value: VehicleType.Sedan, label: 'Sedan' },
    { value: VehicleType.Compact, label: 'Compact' }
  ];

  constructor(
    private vehicleService: VehicleService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Test API connectivity
    this.vehicleService.getAllVehicles().subscribe({
      next: (vehicles) => {
        console.log('API connectivity test successful, vehicles:', vehicles.length);
      },
      error: (error) => {
        console.error('API connectivity test failed:', error);
      }
    });

    this.route.params.subscribe(params => {
      if (params['id'] && params['id'] !== 'new') {
        this.isEditMode = true;
        this.vehicleId = parseInt(params['id'], 10);
        this.loadVehicle();
      }
    });
  }

  private loadVehicle(): void {
    if (!this.vehicleId) return;

    this.isLoading = true;
    this.vehicleService.getVehicleById(this.vehicleId).subscribe({
      next: (vehicle) => {
        this.vehicleData = {
          model: vehicle.model,
          type: vehicle.type,
          year: vehicle.year,
          pricePerDay: vehicle.pricePerDay
        };
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading vehicle:', error);
        this.errorMessage = 'Error al cargar el vehículo';
        this.isLoading = false;
      }
    });
  }

  onSubmit(form: NgForm): void {
    if (!form.valid || this.isSubmitting) {
      console.log('Form validation failed:', form.errors);
      return;
    }

    console.log('Submitting vehicle data:', this.vehicleData);
    console.log('Form validation state:', form.valid);
    console.log('Vehicle service validation:', this.vehicleService.validateVehicleData(this.vehicleData));

    // Ensure correct data types for API
    const vehiclePayload = {
      type: this.vehicleData.type,
      model: this.vehicleData.model.trim(),
      year: Number(this.vehicleData.year),
      pricePerDay: Number(this.vehicleData.pricePerDay)
    };

    console.log('Transformed vehicle payload:', vehiclePayload);

    this.isSubmitting = true;
    this.errorMessage = '';

    const operation = this.isEditMode && this.vehicleId
      ? this.vehicleService.updateVehicle(this.vehicleId, vehiclePayload)
      : this.vehicleService.createVehicle(vehiclePayload);

    operation.subscribe({
      next: (vehicle) => {
        console.log('Vehicle saved:', vehicle);
        this.router.navigate(['/admin/vehicles']);
      },
      error: (error) => {
        console.error('Error saving vehicle:', error);
        if (error.message && error.message.includes('validation')) {
          this.errorMessage = 'Datos del vehículo inválidos. Verifica que todos los campos estén completos y sean válidos.';
        } else if (error.message) {
          this.errorMessage = `Error: ${error.message}`;
        } else {
          this.errorMessage = 'Error al guardar el vehículo. Por favor, inténtalo de nuevo.';
        }
        this.isSubmitting = false;
      }
    });
  }


  goBack(): void {
    this.router.navigate(['/admin/vehicles']);
  }

  get pageTitle(): string {
    return this.isEditMode ? 'Editar Vehículo' : 'Nuevo Vehículo';
  }

  get submitButtonText(): string {
    if (this.isSubmitting) {
      return this.isEditMode ? 'Actualizando...' : 'Creando...';
    }
    return this.isEditMode ? 'Actualizar Vehículo' : 'Crear Vehículo';
  }

  getCurrentYear(): number {
    return new Date().getFullYear();
  }

}