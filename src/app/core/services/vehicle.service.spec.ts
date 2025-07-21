import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { VehicleService } from './vehicle.service';
import { ApiService } from './api.service';
import { of } from 'rxjs';
import { Vehicle, VehicleAvailabilityRequest, VehicleType } from '../../shared/models';

describe('VehicleService', () => {
  let service: VehicleService;
  let apiServiceSpy: jasmine.SpyObj<ApiService>;

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

  beforeEach(() => {
    const spy = jasmine.createSpyObj('ApiService', [
      'getVehicles', 
      'getVehicle', 
      'createVehicle', 
      'updateVehicle', 
      'deleteVehicle',
      'getAvailableVehicles'
    ]);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        VehicleService,
        { provide: ApiService, useValue: spy }
      ]
    });

    service = TestBed.inject(VehicleService);
    apiServiceSpy = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Vehicle CRUD Operations', () => {
    it('should get all vehicles', () => {
      apiServiceSpy.getVehicles.and.returnValue(of(mockVehicles));

      service.getAllVehicles().subscribe(vehicles => {
        expect(vehicles).toEqual(mockVehicles);
        expect(vehicles.length).toBe(2);
      });

      expect(apiServiceSpy.getVehicles).toHaveBeenCalled();
    });

    it('should get vehicle by id', () => {
      const vehicleId = 1;
      const expectedVehicle = mockVehicles[0];
      apiServiceSpy.getVehicle.and.returnValue(of(expectedVehicle));

      service.getVehicleById(vehicleId).subscribe(vehicle => {
        expect(vehicle).toEqual(expectedVehicle);
      });

      expect(apiServiceSpy.getVehicle).toHaveBeenCalledWith(vehicleId);
    });

    it('should create a vehicle', () => {
      const newVehicle = {
        model: 'New Car',
        type: 'Sedan',
        year: 2024,
        seats: 5,
        transmission: 'Manual',
        fuelType: 'Gasolina',
        pricePerDay: 90000,
        available: true,
        imageUrl: 'new-car.jpg',
        features: ['Aire Acondicionado']
      };
      const createdVehicle = { id: 3, ...newVehicle, createdAt: new Date() };
      apiServiceSpy.createVehicle.and.returnValue(of(createdVehicle));

      service.createVehicle(newVehicle).subscribe(vehicle => {
        expect(vehicle.id).toBe(3);
        expect(vehicle.model).toBe('New Car');
      });

      expect(apiServiceSpy.createVehicle).toHaveBeenCalledWith(newVehicle);
    });

    it('should update a vehicle', () => {
      const vehicleId = 1;
      const updateData = {
        model: 'Updated Car',
        type: 'SUV',
        year: 2024,
        seats: 7,
        transmission: 'Automática',
        fuelType: 'Híbrido',
        pricePerDay: 95000,
        available: true,
        imageUrl: 'updated-car.jpg',
        features: ['GPS', 'Bluetooth']
      };
      const updatedVehicle = { id: vehicleId, ...updateData, createdAt: new Date() };
      apiServiceSpy.updateVehicle.and.returnValue(of(updatedVehicle));

      service.updateVehicle(vehicleId, updateData).subscribe(vehicle => {
        expect(vehicle.model).toBe('Updated Car');
        expect(vehicle.type).toBe('SUV');
      });

      expect(apiServiceSpy.updateVehicle).toHaveBeenCalledWith(vehicleId, updateData);
    });

    it('should delete a vehicle', () => {
      const vehicleId = 1;
      apiServiceSpy.deleteVehicle.and.returnValue(of(undefined));

      service.deleteVehicle(vehicleId).subscribe(result => {
        expect(result).toBeUndefined();
      });

      expect(apiServiceSpy.deleteVehicle).toHaveBeenCalledWith(vehicleId);
    });
  });

  describe('Vehicle Availability', () => {
    it('should get available vehicles', () => {
      const availabilityRequest: VehicleAvailabilityRequest = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-07'),
        type: VehicleType.SUV
      };
      const availableVehicles = [mockVehicles[1]]; // Only SUV
      apiServiceSpy.getAvailableVehicles.and.returnValue(of(availableVehicles));

      service.getAvailableVehicles(availabilityRequest).subscribe(vehicles => {
        expect(vehicles).toEqual(availableVehicles);
        expect(vehicles.length).toBe(1);
        expect(vehicles[0].type).toBe('SUV');
      });

      expect(apiServiceSpy.getAvailableVehicles).toHaveBeenCalledWith(
        availabilityRequest.startDate,
        availabilityRequest.endDate,
        availabilityRequest.type
      );
    });

    it('should get available vehicles without type filter', () => {
      const availabilityRequest: VehicleAvailabilityRequest = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-07')
      };
      apiServiceSpy.getAvailableVehicles.and.returnValue(of(mockVehicles));

      service.getAvailableVehicles(availabilityRequest).subscribe(vehicles => {
        expect(vehicles).toEqual(mockVehicles);
      });

      expect(apiServiceSpy.getAvailableVehicles).toHaveBeenCalledWith(
        availabilityRequest.startDate,
        availabilityRequest.endDate,
        undefined
      );
    });
  });

  describe('Price Calculations', () => {
    it('should calculate total price correctly', () => {
      const vehicle = mockVehicles[0]; // 80000 per day
      const startDate = new Date('2024-01-01');
      const endDate = new Date('2024-01-08'); // 7 days

      const totalPrice = service.calculateTotalPrice(vehicle, startDate, endDate);

      expect(totalPrice).toBe(560000); // 80000 * 7 days
    });

    it('should handle same day rental', () => {
      const vehicle = mockVehicles[0];
      const startDate = new Date('2024-01-01');
      const endDate = new Date('2024-01-01'); // Same day

      const totalPrice = service.calculateTotalPrice(vehicle, startDate, endDate);

      expect(totalPrice).toBe(0); // 0 days difference = 0 cost
    });

    it('should format price correctly', () => {
      const price = 120000;
      const formattedPrice = service.formatPrice(price);

      expect(formattedPrice).toBe('$\u00A0120.000,00');
    });

    it('should format large prices correctly', () => {
      const price = 1500000;
      const formattedPrice = service.formatPrice(price);

      expect(formattedPrice).toBe('$\u00A01.500.000,00');
    });
  });

  describe('Vehicle Filtering', () => {
    it('should filter vehicles by type', () => {
      const filteredVehicles = service.filterVehiclesByType(mockVehicles, 'SUV');

      expect(filteredVehicles.length).toBe(1);
      expect(filteredVehicles[0].type).toBe('SUV');
    });

    it('should filter vehicles by availability', () => {
      const mixedVehicles = [
        { ...mockVehicles[0], available: true },
        { ...mockVehicles[1], available: false }
      ];

      const availableVehicles = service.filterAvailableVehicles(mixedVehicles);

      expect(availableVehicles.length).toBe(1);
      expect(availableVehicles[0].available).toBe(true);
    });

    it('should filter vehicles by price range', () => {
      const filteredVehicles = service.filterVehiclesByPriceRange(mockVehicles, 70000, 100000);

      expect(filteredVehicles.length).toBe(1);
      expect(filteredVehicles[0].pricePerDay).toBe(80000);
    });
  });

  describe('Vehicle Validation', () => {
    it('should validate vehicle data correctly', () => {
      const validVehicle = {
        model: 'Test Car',
        type: 'Sedan',
        year: 2024,
        seats: 5,
        transmission: 'Automática',
        fuelType: 'Gasolina',
        pricePerDay: 80000,
        available: true
      };

      const isValid = service.validateVehicleData(validVehicle);
      expect(isValid).toBe(true);
    });

    it('should reject invalid vehicle data', () => {
      const invalidVehicle = {
        model: '', // Empty model
        type: 'Sedan',
        year: 1990, // Too old
        seats: 0, // Invalid seats
        transmission: 'Automática',
        fuelType: 'Gasolina',
        pricePerDay: -1000, // Negative price
        available: true
      };

      const isValid = service.validateVehicleData(invalidVehicle);
      expect(isValid).toBe(false);
    });
  });
});