import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ApiService } from './api.service';
import { ApiResponse } from '../../shared/models';

describe('ApiService', () => {
  let service: ApiService;
  let httpMock: HttpTestingController;
  const vehicleServiceUrl = 'http://localhost:5002/api';
  const bookingServiceUrl = 'http://localhost:5257/api';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ApiService]
    });
    service = TestBed.inject(ApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Vehicle Service Methods', () => {
    it('should get all vehicles', () => {
      const mockVehicles = [
        { id: 1, model: 'Toyota Corolla', type: 'Sedan', year: 2024, pricePerDay: 80000 },
        { id: 2, model: 'Honda CR-V', type: 'SUV', year: 2023, pricePerDay: 120000 }
      ];
      const mockResponse: ApiResponse = {
        success: true,
        data: mockVehicles,
        message: 'Success'
      };

      service.getVehicles().subscribe(vehicles => {
        expect(vehicles).toEqual(mockVehicles);
        expect(vehicles.length).toBe(2);
      });

      const req = httpMock.expectOne(`${vehicleServiceUrl}/vehicles`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should get vehicle by id', () => {
      const mockVehicle = { id: 1, model: 'Toyota Corolla', type: 'Sedan', year: 2024, pricePerDay: 80000 };
      const mockResponse: ApiResponse = {
        success: true,
        data: mockVehicle,
        message: 'Success'
      };

      service.getVehicle(1).subscribe(vehicle => {
        expect(vehicle).toEqual(mockVehicle);
      });

      const req = httpMock.expectOne(`${vehicleServiceUrl}/vehicles/1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should create a new vehicle', () => {
      const newVehicle = { model: 'New Car', type: 'Sedan', year: 2024, pricePerDay: 90000 };
      const mockResponse: ApiResponse = {
        success: true,
        data: { id: 3, ...newVehicle },
        message: 'Vehicle created'
      };

      service.createVehicle(newVehicle).subscribe(vehicle => {
        expect(vehicle.id).toBe(3);
        expect(vehicle.model).toBe('New Car');
      });

      const req = httpMock.expectOne(`${vehicleServiceUrl}/vehicles`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newVehicle);
      req.flush(mockResponse);
    });

    it('should update a vehicle', () => {
      const updatedVehicle = { model: 'Updated Car', type: 'SUV', year: 2024, pricePerDay: 95000 };
      const mockResponse: ApiResponse = {
        success: true,
        data: { id: 1, ...updatedVehicle },
        message: 'Vehicle updated'
      };

      service.updateVehicle(1, updatedVehicle).subscribe(vehicle => {
        expect(vehicle.model).toBe('Updated Car');
        expect(vehicle.type).toBe('SUV');
      });

      const req = httpMock.expectOne(`${vehicleServiceUrl}/vehicles/1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updatedVehicle);
      req.flush(mockResponse);
    });

    it('should delete a vehicle', () => {
      const mockResponse: ApiResponse = {
        success: true,
        message: 'Vehicle deleted'
      };

      service.deleteVehicle(1).subscribe(result => {
        expect(result).toBeUndefined();
      });

      const req = httpMock.expectOne(`${vehicleServiceUrl}/vehicles/1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(mockResponse);
    });

    it('should get available vehicles', () => {
      const startDate = new Date('2024-01-01');
      const endDate = new Date('2024-01-07');
      const type = 'SUV';
      const mockVehicles = [
        { id: 2, model: 'Honda CR-V', type: 'SUV', year: 2023, pricePerDay: 120000 }
      ];
      const mockResponse: ApiResponse = {
        success: true,
        data: mockVehicles,
        message: 'Available vehicles found'
      };

      service.getAvailableVehicles(startDate, endDate, type).subscribe(vehicles => {
        expect(vehicles).toEqual(mockVehicles);
        expect(vehicles.length).toBe(1);
      });

      const req = httpMock.expectOne(request => 
        request.url === `${vehicleServiceUrl}/vehicles/availability` &&
        request.params.get('startDate') === startDate.toISOString() &&
        request.params.get('endDate') === endDate.toISOString() &&
        request.params.get('type') === type
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('Booking Service Methods', () => {
    it('should create a reservation', () => {
      const newReservation = {
        vehicleId: 1,
        customerId: 1,
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-07'),
        totalPrice: 560000
      };
      const mockResponse: ApiResponse = {
        success: true,
        data: { id: 1, ...newReservation, confirmationNumber: 'RF123456' },
        message: 'Reservation created'
      };

      service.createReservation(newReservation).subscribe(reservation => {
        expect(reservation.id).toBe(1);
        expect(reservation.confirmationNumber).toBe('RF123456');
      });

      const req = httpMock.expectOne(`${bookingServiceUrl}/reservations`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newReservation);
      req.flush(mockResponse);
    });

    it('should get reservation by id', () => {
      const mockReservation = {
        id: 1,
        vehicleId: 1,
        customerId: 1,
        confirmationNumber: 'RF123456',
        status: 'Confirmed'
      };
      const mockResponse: ApiResponse = {
        success: true,
        data: mockReservation,
        message: 'Reservation found'
      };

      service.getReservation(1).subscribe(reservation => {
        expect(reservation).toEqual(mockReservation);
      });

      const req = httpMock.expectOne(`${bookingServiceUrl}/reservations/1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should check vehicle availability', () => {
      const vehicleId = 1;
      const startDate = new Date('2024-01-01');
      const endDate = new Date('2024-01-07');
      const mockResponse: ApiResponse<boolean> = {
        success: true,
        data: true,
        message: 'Vehicle is available'
      };

      service.checkVehicleAvailability(vehicleId, startDate, endDate).subscribe(isAvailable => {
        expect(isAvailable).toBe(true);
      });

      const req = httpMock.expectOne(request => 
        request.url === `${bookingServiceUrl}/reservations/check-availability` &&
        request.params.get('vehicleId') === vehicleId.toString() &&
        request.params.get('startDate') === startDate.toISOString() &&
        request.params.get('endDate') === endDate.toISOString()
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should create a customer', () => {
      const newCustomer = {
        firstName: 'Juan',
        lastName: 'Pérez',
        email: 'juan@example.com',
        phone: '3001234567'
      };
      const mockResponse: ApiResponse = {
        success: true,
        data: { id: 1, ...newCustomer },
        message: 'Customer created'
      };

      service.createCustomer(newCustomer).subscribe(customer => {
        expect(customer.id).toBe(1);
        expect(customer.firstName).toBe('Juan');
      });

      const req = httpMock.expectOne(`${bookingServiceUrl}/customers`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newCustomer);
      req.flush(mockResponse);
    });
  });

  describe('Error Handling', () => {
    it('should handle API errors gracefully', () => {
      const errorResponse = {
        status: 500,
        statusText: 'Internal Server Error'
      };

      service.getVehicles().subscribe({
        next: () => fail('Expected an error'),
        error: (error) => {
          expect(error.message).toContain('500 Internal Server Error');
        }
      });

      const req = httpMock.expectOne(`${vehicleServiceUrl}/vehicles`);
      req.flush('Server Error', errorResponse);
    });

    it('should handle network errors', () => {
      service.getVehicles().subscribe({
        next: () => fail('Expected an error'),
        error: (error) => {
          expect(error).toBeTruthy();
        }
      });

      const req = httpMock.expectOne(`${vehicleServiceUrl}/vehicles`);
      req.error(new ErrorEvent('Network error'));
    });
  });
});