import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Observable, forkJoin } from 'rxjs';
import { VehicleService, ReservationService } from '../../core/services';
import { Vehicle, Reservation } from '../../shared/models';

interface DashboardStats {
  totalVehicles: number;
  availableVehicles: number;
  totalReservations: number;
  activeReservations: number;
  totalRevenue: number;
  monthlyRevenue: number;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit {
  stats: DashboardStats = {
    totalVehicles: 0,
    availableVehicles: 0,
    totalReservations: 0,
    activeReservations: 0,
    totalRevenue: 0,
    monthlyRevenue: 0
  };

  recentReservations: Reservation[] = [];
  recentVehicles: Vehicle[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(
    private vehicleService: VehicleService,
    private reservationService: ReservationService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  private loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    // Simulate loading dashboard data
    // In a real app, you'd have specific endpoints for dashboard stats
    this.vehicleService.getAllVehicles().subscribe({
      next: (vehicles) => {
        // Mock recent reservations for demo purposes
        this.recentReservations = this.createMockReservations();
        this.calculateStats(vehicles, this.recentReservations);
        this.recentVehicles = vehicles.slice(0, 5);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading dashboard data:', error);
        this.errorMessage = 'Error al cargar los datos del dashboard';
        this.isLoading = false;
      }
    });
  }

  private createMockReservations(): Reservation[] {
    return [
      {
        id: 1,
        confirmationNumber: 'RF-20250120-001',
        vehicleId: 1,
        customerId: 1,
        startDate: new Date('2025-01-22'),
        endDate: new Date('2025-01-25'),
        totalPrice: 450000,
        status: 'Confirmed' as any,
        createdAt: new Date('2025-01-20'),
        customer: {
          id: 1,
          name: 'Juan Pérez',
          email: 'juan.perez@email.com',
          phone: '+57 300 123 4567',
          documentNumber: '12345678',
          createdAt: '2025-01-15'
        }
      },
      {
        id: 2,
        confirmationNumber: 'RF-20250120-002',
        vehicleId: 2,
        customerId: 2,
        startDate: new Date('2025-01-25'),
        endDate: new Date('2025-01-28'),
        totalPrice: 380000,
        status: 'Pending' as any,
        createdAt: new Date('2025-01-20'),
        customer: {
          id: 2,
          name: 'María González',
          email: 'maria.gonzalez@email.com',
          phone: '+57 321 987 6543',
          documentNumber: '87654321',
          createdAt: '2025-01-18'
        }
      }
    ];
  }

  private calculateStats(vehicles: Vehicle[], reservations: Reservation[]): void {
    this.stats = {
      totalVehicles: vehicles.length,
      availableVehicles: vehicles.filter(v => v.available).length,
      totalReservations: reservations.length,
      activeReservations: reservations.filter(r => r.status === 'Confirmed' || r.status === 'Pending').length,
      totalRevenue: reservations.reduce((sum, r) => sum + r.totalPrice, 0),
      monthlyRevenue: this.calculateMonthlyRevenue(reservations)
    };
  }

  private calculateMonthlyRevenue(reservations: Reservation[]): number {
    const currentMonth = new Date().getMonth();
    const currentYear = new Date().getFullYear();
    
    return reservations
      .filter(r => {
        const reservationDate = new Date(r.startDate);
        return reservationDate.getMonth() === currentMonth && 
               reservationDate.getFullYear() === currentYear;
      })
      .reduce((sum, r) => sum + r.totalPrice, 0);
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP'
    }).format(price);
  }

  getVehicleStatusClass(available: boolean): string {
    return available ? 'status-available' : 'status-unavailable';
  }

  getVehicleStatusText(available: boolean): string {
    return available ? 'Disponible' : 'No disponible';
  }

  refreshDashboard(): void {
    this.loadDashboardData();
  }

  navigateToVehicles(): void {
    // Navigation will be handled by routerLink
  }

  navigateToReservations(): void {
    // Navigation will be handled by routerLink
  }

  navigateToReports(): void {
    // Navigation will be handled by routerLink
  }

  getCurrentMonth(): string {
    return new Date().toLocaleString('es-ES', { month: 'long' });
  }
}