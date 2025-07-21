import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { of, throwError } from 'rxjs';
import { AdminDashboardComponent } from './admin-dashboard.component';
import { VehicleService, ReservationService } from '../../core/services';
import { Vehicle, Reservation } from '../../shared/models';

describe('AdminDashboardComponent', () => {
  let component: AdminDashboardComponent;
  let fixture: ComponentFixture<AdminDashboardComponent>;
  let vehicleServiceSpy: jasmine.SpyObj<VehicleService>;
  let reservationServiceSpy: jasmine.SpyObj<ReservationService>;

  const mockVehicles: Vehicle[] = [
    {
      id: 1,
      model: 'Toyota Corolla',
      type: 'Sedan',
      year: 2024,
      seats: 5,
      transmission: 'Automática',
      fuelType: 'Gasolina',
      pricePerDay: 80000,
      available: true,
      createdAt: new Date(),
      imageUrl: 'test-image.jpg',
      features: ['Aire Acondicionado', 'Bluetooth']
    },
    {
      id: 2,
      model: 'Honda CR-V',
      type: 'SUV',
      year: 2023,
      seats: 7,
      transmission: 'Automática',
      fuelType: 'Gasolina',
      pricePerDay: 120000,
      available: false,
      createdAt: new Date(),
      imageUrl: 'test-image2.jpg',
      features: ['GPS', 'Cámara de Reversa']
    },
    {
      id: 3,
      model: 'Chevrolet Spark',
      type: 'Compact',
      year: 2024,
      seats: 4,
      transmission: 'Manual',
      fuelType: 'Gasolina',
      pricePerDay: 60000,
      available: true,
      createdAt: new Date(),
      imageUrl: 'test-image3.jpg',
      features: ['Radio', 'USB']
    }
  ];

  beforeEach(async () => {
    const vehicleSpy = jasmine.createSpyObj('VehicleService', ['getAllVehicles']);
    const reservationSpy = jasmine.createSpyObj('ReservationService', ['getAllReservations']);

    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        RouterTestingModule,
        AdminDashboardComponent
      ],
      providers: [
        { provide: VehicleService, useValue: vehicleSpy },
        { provide: ReservationService, useValue: reservationSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminDashboardComponent);
    component = fixture.componentInstance;
    vehicleServiceSpy = TestBed.inject(VehicleService) as jasmine.SpyObj<VehicleService>;
    reservationServiceSpy = TestBed.inject(ReservationService) as jasmine.SpyObj<ReservationService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should initialize with default stats and empty arrays', () => {
      expect(component.stats.totalVehicles).toBe(0);
      expect(component.stats.availableVehicles).toBe(0);
      expect(component.stats.totalReservations).toBe(0);
      expect(component.stats.activeReservations).toBe(0);
      expect(component.stats.totalRevenue).toBe(0);
      expect(component.stats.monthlyRevenue).toBe(0);
      expect(component.recentReservations).toEqual([]);
      expect(component.recentVehicles).toEqual([]);
      expect(component.isLoading).toBe(true);
      expect(component.errorMessage).toBe('');
    });

    it('should load dashboard data on init', () => {
      vehicleServiceSpy.getAllVehicles.and.returnValue(of(mockVehicles));
      
      component.ngOnInit();
      
      expect(vehicleServiceSpy.getAllVehicles).toHaveBeenCalled();
      expect(component.isLoading).toBe(false);
    });
  });

  describe('Dashboard Data Loading', () => {
    it('should load and calculate stats correctly', () => {
      vehicleServiceSpy.getAllVehicles.and.returnValue(of(mockVehicles));
      
      component.ngOnInit();
      
      expect(component.stats.totalVehicles).toBe(3);
      expect(component.stats.availableVehicles).toBe(2); // 2 available vehicles
      expect(component.recentVehicles.length).toBe(3); // All vehicles are shown as recent (≤5)
      expect(component.isLoading).toBe(false);
      expect(component.errorMessage).toBe('');
    });

    it('should limit recent vehicles to 5', () => {
      const manyVehicles = Array(10).fill(0).map((_, i) => ({
        ...mockVehicles[0],
        id: i + 1,
        model: `Vehicle ${i + 1}`
      }));
      vehicleServiceSpy.getAllVehicles.and.returnValue(of(manyVehicles));
      
      component.ngOnInit();
      
      expect(component.recentVehicles.length).toBe(5);
    });

    it('should handle loading errors', () => {
      const errorMessage = 'API Error';
      vehicleServiceSpy.getAllVehicles.and.returnValue(throwError(() => new Error(errorMessage)));
      spyOn(console, 'error');
      
      component.ngOnInit();
      
      expect(component.errorMessage).toBe('Error al cargar los datos del dashboard');
      expect(component.isLoading).toBe(false);
      expect(console.error).toHaveBeenCalled();
    });
  });

  describe('Mock Reservations Creation', () => {
    it('should create mock reservations with correct structure', () => {
      const mockReservations = (component as any).createMockReservations();
      
      expect(mockReservations.length).toBe(2);
      expect(mockReservations[0].confirmationNumber).toBe('RF-20250120-001');
      expect(mockReservations[0].customer).toBeDefined();
      expect(mockReservations[0].customer.firstName).toBe('Juan');
      expect(mockReservations[1].customer.firstName).toBe('María');
    });
  });

  describe('Stats Calculations', () => {
    it('should calculate stats correctly', () => {
      const mockReservations = [
        {
          id: 1,
          totalPrice: 450000,
          status: 'Confirmed' as any,
          startDate: new Date(),
          endDate: new Date(),
          confirmationNumber: 'RF001',
          vehicleId: 1,
          customerId: 1,
          createdAt: new Date()
        },
        {
          id: 2,
          totalPrice: 380000,
          status: 'Pending' as any,
          startDate: new Date(),
          endDate: new Date(),
          confirmationNumber: 'RF002',
          vehicleId: 2,
          customerId: 2,
          createdAt: new Date()
        }
      ];

      (component as any).calculateStats(mockVehicles, mockReservations);
      
      expect(component.stats.totalVehicles).toBe(3);
      expect(component.stats.availableVehicles).toBe(2);
      expect(component.stats.totalReservations).toBe(2);
      expect(component.stats.activeReservations).toBe(2); // Both confirmed and pending
      expect(component.stats.totalRevenue).toBe(830000);
    });

    it('should calculate monthly revenue correctly', () => {
      const currentDate = new Date();
      const currentMonth = currentDate.getMonth();
      const currentYear = currentDate.getFullYear();
      
      const mockReservations = [
        {
          id: 1,
          totalPrice: 450000,
          status: 'Confirmed' as any,
          startDate: new Date(currentYear, currentMonth, 15), // This month
          endDate: new Date(),
          confirmationNumber: 'RF001',
          vehicleId: 1,
          customerId: 1,
          createdAt: new Date()
        },
        {
          id: 2,
          totalPrice: 380000,
          status: 'Confirmed' as any,
          startDate: new Date(currentYear, currentMonth - 1, 15), // Last month
          endDate: new Date(),
          confirmationNumber: 'RF002',
          vehicleId: 2,
          customerId: 2,
          createdAt: new Date()
        }
      ];

      const monthlyRevenue = (component as any).calculateMonthlyRevenue(mockReservations);
      
      expect(monthlyRevenue).toBe(450000); // Only current month reservation
    });
  });

  describe('Utility Methods', () => {
    it('should format price correctly', () => {
      const price = 450000;
      const formatted = component.formatPrice(price);
      
      expect(formatted).toBe('$\u00A0450.000,00');
    });

    it('should return correct vehicle status class', () => {
      expect(component.getVehicleStatusClass(true)).toBe('status-available');
      expect(component.getVehicleStatusClass(false)).toBe('status-unavailable');
    });

    it('should return correct vehicle status text', () => {
      expect(component.getVehicleStatusText(true)).toBe('Disponible');
      expect(component.getVehicleStatusText(false)).toBe('No disponible');
    });

    it('should get current month in Spanish', () => {
      const month = component.getCurrentMonth();
      
      expect(typeof month).toBe('string');
      expect(month.length).toBeGreaterThan(0);
      // Should be a valid Spanish month name
      const validMonths = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio',
                          'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];
      expect(validMonths).toContain(month);
    });
  });

  describe('Dashboard Actions', () => {
    it('should refresh dashboard data', () => {
      vehicleServiceSpy.getAllVehicles.and.returnValue(of(mockVehicles));
      spyOn(component, 'loadDashboardData' as any).and.callThrough();
      
      component.refreshDashboard();
      
      expect((component as any).loadDashboardData).toHaveBeenCalled();
    });

    it('should have navigation methods defined', () => {
      expect(component.navigateToVehicles).toBeDefined();
      expect(component.navigateToReservations).toBeDefined();
      expect(component.navigateToReports).toBeDefined();
      
      // These methods don't do anything (navigation handled by routerLink)
      // but they should exist and not throw errors
      expect(() => component.navigateToVehicles()).not.toThrow();
      expect(() => component.navigateToReservations()).not.toThrow();
      expect(() => component.navigateToReports()).not.toThrow();
    });
  });

  describe('Edge Cases', () => {
    it('should handle empty vehicle list', () => {
      vehicleServiceSpy.getAllVehicles.and.returnValue(of([]));
      
      component.ngOnInit();
      
      expect(component.stats.totalVehicles).toBe(0);
      expect(component.stats.availableVehicles).toBe(0);
      expect(component.recentVehicles).toEqual([]);
    });

    it('should handle all vehicles unavailable', () => {
      const unavailableVehicles = mockVehicles.map(v => ({ ...v, available: false }));
      vehicleServiceSpy.getAllVehicles.and.returnValue(of(unavailableVehicles));
      
      component.ngOnInit();
      
      expect(component.stats.totalVehicles).toBe(3);
      expect(component.stats.availableVehicles).toBe(0);
    });

    it('should handle reservations with different statuses', () => {
      const mockReservations = [
        {
          id: 1,
          totalPrice: 450000,
          status: 'Confirmed' as any,
          startDate: new Date(),
          endDate: new Date(),
          confirmationNumber: 'RF001',
          vehicleId: 1,
          customerId: 1,
          createdAt: new Date()
        },
        {
          id: 2,
          totalPrice: 380000,
          status: 'Cancelled' as any,
          startDate: new Date(),
          endDate: new Date(),
          confirmationNumber: 'RF002',
          vehicleId: 2,
          customerId: 2,
          createdAt: new Date()
        }
      ];

      (component as any).calculateStats(mockVehicles, mockReservations);
      
      expect(component.stats.activeReservations).toBe(1); // Only confirmed ones
      expect(component.stats.totalReservations).toBe(2); // All reservations
    });
  });
});