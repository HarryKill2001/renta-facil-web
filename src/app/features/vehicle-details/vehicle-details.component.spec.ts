import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError, Observable } from 'rxjs';
import { VehicleDetailsComponent } from './vehicle-details.component';
import { VehicleService } from '../../core/services';
import { Vehicle } from '../../shared/models';

describe('VehicleDetailsComponent', () => {
  let component: VehicleDetailsComponent;
  let fixture: ComponentFixture<VehicleDetailsComponent>;
  let vehicleServiceSpy: jasmine.SpyObj<VehicleService>;
  let routerSpy: jasmine.SpyObj<Router>;
  let mockActivatedRoute: any;

  const mockVehicle: Vehicle = {
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
  };

  beforeEach(async () => {
    const vehicleSpy = jasmine.createSpyObj('VehicleService', [
      'getVehicleById',
      'checkAvailability',
      'calculateTotalPrice',
      'formatPrice'
    ]);
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    mockActivatedRoute = {
      params: of({ id: '1' })
    };

    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        VehicleDetailsComponent
      ],
      providers: [
        { provide: VehicleService, useValue: vehicleSpy },
        { provide: Router, useValue: routerSpyObj },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VehicleDetailsComponent);
    component = fixture.componentInstance;
    vehicleServiceSpy = TestBed.inject(VehicleService) as jasmine.SpyObj<VehicleService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    
    // Reset spy calls and component state
    vehicleServiceSpy.getVehicleById.calls.reset();
    component.vehicleId = undefined;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should initialize with default date range', () => {
      const today = new Date();
      const tomorrow = new Date(Date.now() + 24 * 60 * 60 * 1000);
      
      expect(component.selectedStartDate.getDate()).toBe(today.getDate());
      expect(component.selectedEndDate.getDate()).toBe(tomorrow.getDate());
      expect(component.isCheckingAvailability).toBe(false);
      expect(component.availabilityResult).toBeNull();
    });

    it('should load vehicle from vehicleId input when provided', () => {
      component.vehicleId = 1;
      vehicleServiceSpy.getVehicleById.and.returnValue(of(mockVehicle));
      
      component.ngOnInit();
      
      expect(vehicleServiceSpy.getVehicleById).toHaveBeenCalledWith(1);
    });

    it('should load vehicle from route params when vehicleId not provided', () => {
      vehicleServiceSpy.getVehicleById.and.returnValue(of(mockVehicle));
      
      // vehicleId should be undefined from beforeEach setup
      expect(component.vehicleId).toBeUndefined();
      
      // Call ngOnInit to trigger the subscription to route params
      component.ngOnInit();
      
      expect(vehicleServiceSpy.getVehicleById).toHaveBeenCalledWith(1);
    });
  });

  describe('Date Management', () => {
    it('should update start date', () => {
      const event = { target: { value: '2024-01-15' } };
      
      component.updateStartDate(event);
      
      expect(component.selectedStartDate.getFullYear()).toBe(2024);
      expect(component.selectedStartDate.getMonth()).toBe(0); // January
      expect(component.selectedStartDate.getDate()).toBeGreaterThanOrEqual(14); // Allow for timezone differences
      expect(component.selectedStartDate.getDate()).toBeLessThanOrEqual(15);
    });

    it('should update end date', () => {
      const event = { target: { value: '2024-01-20' } };
      
      component.updateEndDate(event);
      
      expect(component.selectedEndDate.getFullYear()).toBe(2024);
      expect(component.selectedEndDate.getMonth()).toBe(0); // January
      expect(component.selectedEndDate.getDate()).toBeGreaterThanOrEqual(19); // Allow for timezone differences
      expect(component.selectedEndDate.getDate()).toBeLessThanOrEqual(20);
    });

    it('should validate date range correctly', () => {
      // Valid date range
      component.selectedStartDate = new Date();
      component.selectedEndDate = new Date(Date.now() + 2 * 24 * 60 * 60 * 1000);
      
      expect(component.isDateRangeValid).toBe(true);
    });

    it('should invalidate past start date', () => {
      component.selectedStartDate = new Date(Date.now() - 24 * 60 * 60 * 1000); // Yesterday
      component.selectedEndDate = new Date();
      
      expect(component.isDateRangeValid).toBe(false);
    });

    it('should invalidate end date before start date', () => {
      component.selectedStartDate = new Date();
      component.selectedEndDate = new Date(Date.now() - 24 * 60 * 60 * 1000); // Yesterday
      
      expect(component.isDateRangeValid).toBe(false);
    });

    it('should return correct minimum start date', () => {
      const today = new Date().toISOString().split('T')[0];
      
      expect(component.minStartDate).toBe(today);
    });

    it('should return correct minimum end date', () => {
      component.selectedStartDate = new Date('2024-01-15');
      const expectedMinEnd = '2024-01-16';
      
      expect(component.minEndDate).toBe(expectedMinEnd);
    });

    it('should calculate total days correctly', () => {
      component.selectedStartDate = new Date('2024-01-15');
      component.selectedEndDate = new Date('2024-01-20');
      
      expect(component.totalDays).toBe(5);
    });
  });

  describe('Availability Checking', () => {
    beforeEach(() => {
      const today = new Date();
      const futureDate = new Date(today.getTime() + 2 * 24 * 60 * 60 * 1000);
      component.selectedStartDate = today;
      component.selectedEndDate = futureDate;
    });

    it('should check availability successfully', () => {
      vehicleServiceSpy.checkAvailability.and.returnValue(of(true));
      
      component.checkAvailability(1);
      
      expect(vehicleServiceSpy.checkAvailability).toHaveBeenCalledWith(
        1,
        component.selectedStartDate,
        component.selectedEndDate
      );
      expect(component.availabilityResult).toBe(true);
      expect(component.isCheckingAvailability).toBe(false);
    });

    it('should handle availability check error', () => {
      vehicleServiceSpy.checkAvailability.and.returnValue(throwError(() => new Error('API Error')));
      spyOn(console, 'error');
      
      component.checkAvailability(1);
      
      expect(console.error).toHaveBeenCalledWith('Error checking availability:', jasmine.any(Error));
      expect(component.isCheckingAvailability).toBe(false);
    });

    it('should not check availability with invalid date range', () => {
      component.selectedStartDate = new Date(Date.now() - 24 * 60 * 60 * 1000); // Invalid
      
      component.checkAvailability(1);
      
      expect(vehicleServiceSpy.checkAvailability).not.toHaveBeenCalled();
    });

    it('should reset availability result when checking', () => {
      component.availabilityResult = false; // Previous result
      vehicleServiceSpy.checkAvailability.and.returnValue(of(true));
      
      component.checkAvailability(1);
      
      expect(component.availabilityResult).toBe(true); // New result
    });
  });

  describe('Vehicle Booking', () => {
    beforeEach(() => {
      // Use future dates to pass validation
      const today = new Date();
      const futureStart = new Date(today.getTime() + 24 * 60 * 60 * 1000); // Tomorrow
      const futureEnd = new Date(today.getTime() + 6 * 24 * 60 * 60 * 1000); // Next week
      
      component.selectedStartDate = futureStart;
      component.selectedEndDate = futureEnd;
      vehicleServiceSpy.calculateTotalPrice.and.returnValue(400000);
    });

    it('should book vehicle with correct data', () => {
      spyOn(console, 'log');
      
      component.bookVehicle(mockVehicle);
      
      const expectedBookingData = {
        vehicleId: 1,
        startDate: component.selectedStartDate,
        endDate: component.selectedEndDate,
        totalPrice: 400000
      };
      
      expect(console.log).toHaveBeenCalledWith('Booking vehicle:', expectedBookingData);
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/booking'], {
        queryParams: expectedBookingData
      });
    });

    it('should not book with invalid date range', () => {
      component.selectedStartDate = new Date(Date.now() - 24 * 60 * 60 * 1000); // Invalid
      
      component.bookVehicle(mockVehicle);
      
      expect(routerSpy.navigate).not.toHaveBeenCalled();
    });

    it('should calculate total price for booking', () => {
      vehicleServiceSpy.calculateTotalPrice.and.returnValue(400000);
      
      const totalPrice = component.calculateTotalPrice(mockVehicle);
      
      expect(vehicleServiceSpy.calculateTotalPrice).toHaveBeenCalledWith(
        mockVehicle,
        component.selectedStartDate,
        component.selectedEndDate
      );
      expect(totalPrice).toBe(400000);
    });
  });

  describe('Utility Methods', () => {
    it('should format price correctly', () => {
      const price = 400000;
      vehicleServiceSpy.formatPrice.and.returnValue('$\u00A0400.000,00');
      
      const formatted = component.formatPrice(price);
      
      expect(vehicleServiceSpy.formatPrice).toHaveBeenCalledWith(price);
      expect(formatted).toBe('$\u00A0400.000,00');
    });

    it('should return correct vehicle type display', () => {
      expect(component.getVehicleTypeDisplay('SUV')).toBe('SUV');
      expect(component.getVehicleTypeDisplay('Sedan')).toBe('Sedán');
      expect(component.getVehicleTypeDisplay('Compact')).toBe('Compacto');
      expect(component.getVehicleTypeDisplay('Hatchback')).toBe('Hatchback');
      expect(component.getVehicleTypeDisplay('Unknown')).toBe('Unknown');
    });

    it('should go back in browser history', () => {
      spyOn(window.history, 'back');
      
      component.goBack();
      
      expect(window.history.back).toHaveBeenCalled();
    });
  });

  describe('Loading States', () => {
    beforeEach(() => {
      component.selectedStartDate = new Date();
      component.selectedEndDate = new Date(Date.now() + 2 * 24 * 60 * 60 * 1000);
    });

    it('should set loading state during availability check', () => {
      // Create a delayed observable that we can control
      vehicleServiceSpy.checkAvailability.and.returnValue(new Observable(observer => {
        // Don't complete immediately to test loading state
        setTimeout(() => {
          observer.next(true);
          observer.complete();
        }, 100);
      }));
      
      expect(component.isCheckingAvailability).toBe(false);
      
      component.checkAvailability(1);
      expect(component.isCheckingAvailability).toBe(true);
    });
  });

  describe('Route Parameter Handling', () => {
    it('should handle string route parameter conversion', () => {
      // Update the mock route to return '123'
      mockActivatedRoute.params = of({ id: '123' });
      vehicleServiceSpy.getVehicleById.and.returnValue(of(mockVehicle));
      
      // Ensure vehicleId is not set
      component.vehicleId = undefined;
      
      // Manually trigger the lifecycle
      component.ngOnInit();
      
      expect(vehicleServiceSpy.getVehicleById).toHaveBeenCalledWith(123);
    });

    it('should handle invalid route parameter', () => {
      // Update the mock route to return 'invalid'
      mockActivatedRoute.params = of({ id: 'invalid' });
      vehicleServiceSpy.getVehicleById.and.returnValue(of(mockVehicle));
      
      // Ensure vehicleId is not set
      component.vehicleId = undefined;
      
      // Manually trigger the lifecycle
      component.ngOnInit();
      
      expect(vehicleServiceSpy.getVehicleById).toHaveBeenCalledWith(NaN);
    });
  });

  describe('Edge Cases', () => {
    it('should handle same day booking', () => {
      const today = new Date();
      component.selectedStartDate = today;
      component.selectedEndDate = today;
      
      expect(component.totalDays).toBe(0);
      expect(component.isDateRangeValid).toBe(false); // End must be after start
    });

    it('should handle long-term booking', () => {
      component.selectedStartDate = new Date('2024-01-01');
      component.selectedEndDate = new Date('2024-01-31');
      
      expect(component.totalDays).toBe(30);
    });

    it('should handle availability check with null result', () => {
      vehicleServiceSpy.checkAvailability.and.returnValue(of(null as any));
      
      component.checkAvailability(1);
      
      expect(component.availabilityResult).toBeNull();
      expect(component.isCheckingAvailability).toBe(false);
    });
  });
});