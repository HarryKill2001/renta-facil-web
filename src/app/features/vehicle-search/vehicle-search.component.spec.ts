import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { of, throwError } from 'rxjs';
import { VehicleSearchComponent } from './vehicle-search.component';
import { VehicleService } from '../../core/services';
import { Vehicle, VehicleType, VehicleAvailabilityRequest } from '../../shared/models';
import { VehicleCardSkeletonComponent } from '../../shared/components/vehicle-card-skeleton/vehicle-card-skeleton.component';

describe('VehicleSearchComponent', () => {
  let component: VehicleSearchComponent;
  let fixture: ComponentFixture<VehicleSearchComponent>;
  let vehicleServiceSpy: jasmine.SpyObj<VehicleService>;

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
      available: true,
      createdAt: new Date(),
      imageUrl: 'test-image2.jpg',
      features: ['GPS', 'Cámara de Reversa']
    }
  ];

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('VehicleService', [
      'getAllVehicles',
      'getAvailableVehicles',
      'calculateTotalPrice',
      'formatPrice'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        FormsModule,
        VehicleSearchComponent,
        VehicleCardSkeletonComponent
      ],
      providers: [
        { provide: VehicleService, useValue: spy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VehicleSearchComponent);
    component = fixture.componentInstance;
    vehicleServiceSpy = TestBed.inject(VehicleService) as jasmine.SpyObj<VehicleService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should initialize with default availability request', () => {
      expect(component.availabilityRequest.startDate).toEqual(jasmine.any(Date));
      expect(component.availabilityRequest.endDate).toEqual(jasmine.any(Date));
      expect(component.availabilityRequest.type).toBeUndefined();
      expect(component.isSearching).toBe(false);
      expect(component.hasSearched).toBe(false);
      expect(component.isLoading).toBe(true);
    });

    it('should set end date to tomorrow by default', () => {
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      
      expect(component.availabilityRequest.endDate.getDate()).toBe(tomorrow.getDate());
    });

    it('should load all vehicles on init', () => {
      vehicleServiceSpy.getAllVehicles.and.returnValue(of(mockVehicles));
      
      component.ngOnInit();
      
      expect(vehicleServiceSpy.getAllVehicles).toHaveBeenCalled();
    });
  });

  describe('Vehicle Loading', () => {
    it('should load all vehicles successfully', () => {
      vehicleServiceSpy.getAllVehicles.and.returnValue(of(mockVehicles));
      
      component.loadAllVehicles();
      
      expect(component.isLoading).toBe(true); // Initially set to true
      expect(vehicleServiceSpy.getAllVehicles).toHaveBeenCalled();
      
      // Simulate the timeout completion
      setTimeout(() => {
        expect(component.isLoading).toBe(false);
      }, 1500);
    });

    it('should handle vehicle loading errors', () => {
      const errorMessage = 'Failed to load vehicles';
      vehicleServiceSpy.getAllVehicles.and.returnValue(throwError(() => new Error(errorMessage)));
      
      component.loadAllVehicles();
      
      expect(vehicleServiceSpy.getAllVehicles).toHaveBeenCalled();
    });
  });

  describe('Vehicle Search', () => {
    beforeEach(() => {
      vehicleServiceSpy.getAvailableVehicles.and.returnValue(of(mockVehicles));
    });

    it('should search for available vehicles with valid date range', () => {
      const today = new Date();
      const tomorrow = new Date(today.getTime() + 24 * 60 * 60 * 1000);
      const nextWeek = new Date(today.getTime() + 7 * 24 * 60 * 60 * 1000);
      
      component.availabilityRequest = {
        startDate: tomorrow,
        endDate: nextWeek,
        type: VehicleType.SUV
      };
      
      component.searchAvailableVehicles();
      
      expect(component.isSearching).toBe(true);
      expect(component.isLoading).toBe(true);
      expect(component.hasSearched).toBe(true);
      expect(vehicleServiceSpy.getAvailableVehicles).toHaveBeenCalledWith(component.availabilityRequest);
      
      // Simulate timeout completion
      setTimeout(() => {
        expect(component.isSearching).toBe(false);
        expect(component.isLoading).toBe(false);
      }, 1000);
    });

    it('should not search with invalid date range', () => {
      const today = new Date();
      const tomorrow = new Date(today.getTime() + 24 * 60 * 60 * 1000);
      
      component.availabilityRequest = {
        startDate: tomorrow,
        endDate: today, // End before start
        type: undefined
      };
      
      component.searchAvailableVehicles();
      
      expect(vehicleServiceSpy.getAvailableVehicles).not.toHaveBeenCalled();
      expect(component.isSearching).toBe(false);
    });

    it('should not search with past start date', () => {
      const yesterday = new Date();
      yesterday.setDate(yesterday.getDate() - 1);
      
      component.availabilityRequest = {
        startDate: yesterday,
        endDate: new Date(),
        type: undefined
      };
      
      component.searchAvailableVehicles();
      
      expect(vehicleServiceSpy.getAvailableVehicles).not.toHaveBeenCalled();
    });
  });

  describe('Search Reset', () => {
    it('should reset search parameters', () => {
      // Set some search state
      component.hasSearched = true;
      component.availabilityRequest.type = VehicleType.SUV;
      vehicleServiceSpy.getAllVehicles.and.returnValue(of(mockVehicles));
      
      component.resetSearch();
      
      expect(component.hasSearched).toBe(false);
      expect(component.availabilityRequest.type).toBeUndefined();
      expect(component.availabilityRequest.startDate).toEqual(jasmine.any(Date));
      expect(vehicleServiceSpy.getAllVehicles).toHaveBeenCalled();
    });
  });

  describe('Price Calculations', () => {
    it('should calculate total price for selected vehicle', () => {
      const vehicle = mockVehicles[0];
      const expectedPrice = 560000;
      vehicleServiceSpy.calculateTotalPrice.and.returnValue(expectedPrice);
      
      const totalPrice = component.calculateTotalPrice(vehicle);
      
      expect(vehicleServiceSpy.calculateTotalPrice).toHaveBeenCalledWith(
        vehicle,
        component.availabilityRequest.startDate,
        component.availabilityRequest.endDate
      );
      expect(totalPrice).toBe(expectedPrice);
    });

    it('should format price correctly', () => {
      const price = 120000;
      const formattedPrice = '$120.000';
      vehicleServiceSpy.formatPrice.and.returnValue(formattedPrice);
      
      const result = component.formatPrice(price);
      
      expect(vehicleServiceSpy.formatPrice).toHaveBeenCalledWith(price);
      expect(result).toBe(formattedPrice);
    });
  });

  describe('Vehicle Actions', () => {
    it('should select vehicle and log to console', () => {
      spyOn(console, 'log');
      const vehicle = mockVehicles[0];
      
      component.selectVehicle(vehicle);
      
      expect(console.log).toHaveBeenCalledWith('Selected vehicle:', vehicle);
    });

    it('should view vehicle details and log to console', () => {
      spyOn(console, 'log');
      const vehicle = mockVehicles[0];
      
      component.viewDetails(vehicle);
      
      expect(console.log).toHaveBeenCalledWith('View details for vehicle:', vehicle);
    });
  });

  describe('Vehicle Type Display', () => {
    it('should return correct Spanish display for vehicle types', () => {
      expect(component.getVehicleTypeDisplay('SUV')).toBe('SUV');
      expect(component.getVehicleTypeDisplay('Sedan')).toBe('Sedán');
      expect(component.getVehicleTypeDisplay('Compact')).toBe('Compacto');
      expect(component.getVehicleTypeDisplay('Hatchback')).toBe('Hatchback');
      expect(component.getVehicleTypeDisplay('Camioneta')).toBe('Camioneta');
      expect(component.getVehicleTypeDisplay('Deportivo')).toBe('Deportivo');
      expect(component.getVehicleTypeDisplay('Convertible')).toBe('Convertible');
      expect(component.getVehicleTypeDisplay('Familiar')).toBe('Familiar');
    });

    it('should return original type if not mapped', () => {
      const unknownType = 'Unknown';
      expect(component.getVehicleTypeDisplay(unknownType)).toBe(unknownType);
    });
  });

  describe('Date Validation', () => {
    it('should validate correct date range', () => {
      const today = new Date();
      const tomorrow = new Date();
      tomorrow.setDate(today.getDate() + 1);
      
      component.availabilityRequest = {
        startDate: today,
        endDate: tomorrow,
        type: undefined
      };
      
      // Access private method through any cast for testing
      const isValid = (component as any).isValidDateRange();
      expect(isValid).toBe(true);
    });

    it('should invalidate past start date', () => {
      const yesterday = new Date();
      yesterday.setDate(yesterday.getDate() - 1);
      const today = new Date();
      
      component.availabilityRequest = {
        startDate: yesterday,
        endDate: today,
        type: undefined
      };
      
      const isValid = (component as any).isValidDateRange();
      expect(isValid).toBe(false);
    });

    it('should invalidate end date before start date', () => {
      const today = new Date();
      const yesterday = new Date();
      yesterday.setDate(today.getDate() - 1);
      
      component.availabilityRequest = {
        startDate: today,
        endDate: yesterday,
        type: undefined
      };
      
      const isValid = (component as any).isValidDateRange();
      expect(isValid).toBe(false);
    });
  });

  describe('Minimum Date Getters', () => {
    it('should return today as minimum start date', () => {
      const today = new Date().toISOString().split('T')[0];
      expect(component.minStartDate).toBe(today);
    });

    it('should return tomorrow as minimum end date when start date is today', () => {
      const today = new Date();
      const tomorrow = new Date(today);
      tomorrow.setDate(tomorrow.getDate() + 1);
      
      component.availabilityRequest.startDate = today;
      
      expect(component.minEndDate).toBe(tomorrow.toISOString().split('T')[0]);
    });
  });
});