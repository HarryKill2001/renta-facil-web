import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { BookingFormComponent } from './booking-form.component';
import { VehicleService, CustomerService, ReservationService } from '../../core/services';
import { Vehicle, Customer, CreateReservation, ReservationStatus } from '../../shared/models';

describe('BookingFormComponent', () => {
  let component: BookingFormComponent;
  let fixture: ComponentFixture<BookingFormComponent>;
  let vehicleServiceSpy: jasmine.SpyObj<VehicleService>;
  let customerServiceSpy: jasmine.SpyObj<CustomerService>;
  let reservationServiceSpy: jasmine.SpyObj<ReservationService>;
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

  const mockCustomer: Customer = {
    id: 1,
    firstName: 'Juan',
    lastName: 'Pérez',
    email: 'juan@example.com',
    phone: '3001234567',
    documentType: 'CC',
    documentNumber: '12345678',
    address: 'Calle 123',
    city: 'Bogotá',
    birthDate: new Date('1990-01-01'),
    createdAt: new Date()
  };

  beforeEach(async () => {
    const vehicleSpy = jasmine.createSpyObj('VehicleService', ['getVehicleById', 'formatPrice']);
    const customerSpy = jasmine.createSpyObj('CustomerService', [
      'createCustomer', 
      'validateEmail', 
      'validatePhone', 
      'validateDocumentNumber',
      'formatPhoneNumber'
    ]);
    const reservationSpy = jasmine.createSpyObj('ReservationService', [
      'createReservation', 
      'generateConfirmationNumber'
    ]);
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    mockActivatedRoute = {
      queryParams: of({
        vehicleId: '1',
        startDate: '2024-01-01',
        endDate: '2024-01-07',
        totalPrice: '560000'
      })
    };

    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        FormsModule,
        BookingFormComponent
      ],
      providers: [
        { provide: VehicleService, useValue: vehicleSpy },
        { provide: CustomerService, useValue: customerSpy },
        { provide: ReservationService, useValue: reservationSpy },
        { provide: Router, useValue: routerSpyObj },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BookingFormComponent);
    component = fixture.componentInstance;
    vehicleServiceSpy = TestBed.inject(VehicleService) as jasmine.SpyObj<VehicleService>;
    customerServiceSpy = TestBed.inject(CustomerService) as jasmine.SpyObj<CustomerService>;
    reservationServiceSpy = TestBed.inject(ReservationService) as jasmine.SpyObj<ReservationService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should initialize with default customer data', () => {
      expect(component.customerData.firstName).toBe('');
      expect(component.customerData.documentType).toBe('CC');
      expect(component.isSubmitting).toBe(false);
      expect(component.showSuccessMessage).toBe(false);
    });

    it('should load booking data from query params', () => {
      vehicleServiceSpy.getVehicleById.and.returnValue(of(mockVehicle));
      
      component.ngOnInit();
      
      expect(component.bookingData.vehicleId).toBe(1);
      expect(component.bookingData.totalPrice).toBe(560000);
      expect(vehicleServiceSpy.getVehicleById).toHaveBeenCalledWith(1);
    });

    it('should calculate total days correctly', () => {
      component.bookingData.startDate = new Date('2024-01-01');
      component.bookingData.endDate = new Date('2024-01-08');
      
      expect(component.totalDays).toBe(7);
    });

    it('should return correct max birth date for 18+ validation', () => {
      const expectedDate = new Date();
      expectedDate.setFullYear(expectedDate.getFullYear() - 18);
      const expected = expectedDate.toISOString().split('T')[0];
      
      expect(component.maxBirthDate).toBe(expected);
    });
  });

  describe('Form Validation', () => {
    beforeEach(() => {
      component.customerData = {
        firstName: 'Juan',
        lastName: 'Pérez',
        email: 'juan@example.com',
        phone: '3001234567',
        documentType: 'CC',
        documentNumber: '12345678',
        address: 'Calle 123',
        city: 'Bogotá',
        birthDate: new Date('1990-01-01')
      };
    });

    it('should validate customer data successfully', () => {
      customerServiceSpy.validateEmail.and.returnValue(true);
      customerServiceSpy.validatePhone.and.returnValue(true);
      customerServiceSpy.validateDocumentNumber.and.returnValue(true);
      
      const isValid = (component as any).validateCustomerData();
      
      expect(isValid).toBe(true);
      expect(component.errorMessage).toBe('');
    });

    it('should reject invalid email', () => {
      customerServiceSpy.validateEmail.and.returnValue(false);
      customerServiceSpy.validatePhone.and.returnValue(true);
      customerServiceSpy.validateDocumentNumber.and.returnValue(true);
      
      const isValid = (component as any).validateCustomerData();
      
      expect(isValid).toBe(false);
      expect(component.errorMessage).toContain('email válido');
    });

    it('should reject invalid phone number', () => {
      customerServiceSpy.validateEmail.and.returnValue(true);
      customerServiceSpy.validatePhone.and.returnValue(false);
      customerServiceSpy.validateDocumentNumber.and.returnValue(true);
      
      const isValid = (component as any).validateCustomerData();
      
      expect(isValid).toBe(false);
      expect(component.errorMessage).toContain('teléfono válido');
    });

    it('should reject invalid document number', () => {
      customerServiceSpy.validateEmail.and.returnValue(true);
      customerServiceSpy.validatePhone.and.returnValue(true);
      customerServiceSpy.validateDocumentNumber.and.returnValue(false);
      
      const isValid = (component as any).validateCustomerData();
      
      expect(isValid).toBe(false);
      expect(component.errorMessage).toContain('documento válido');
    });

    it('should reject customers under 18 years old', () => {
      component.customerData.birthDate = new Date(); // Today's date (0 years old)
      customerServiceSpy.validateEmail.and.returnValue(true);
      customerServiceSpy.validatePhone.and.returnValue(true);
      customerServiceSpy.validateDocumentNumber.and.returnValue(true);
      
      const isValid = (component as any).validateCustomerData();
      
      expect(isValid).toBe(false);
      expect(component.errorMessage).toContain('mayor de 18 años');
    });
  });

  describe('Age Calculation', () => {
    it('should calculate age correctly', () => {
      const birthDate = new Date('1990-06-15');
      const age = (component as any).calculateAge(birthDate);
      
      // Age calculation depends on current date, so we'll check it's reasonable
      expect(age).toBeGreaterThan(30);
      expect(age).toBeLessThan(50);
    });

    it('should handle birthday not yet reached this year', () => {
      const today = new Date();
      const birthDate = new Date(today.getFullYear() - 25, today.getMonth() + 1, today.getDate());
      
      const age = (component as any).calculateAge(birthDate);
      
      expect(age).toBe(24); // Birthday hasn't happened yet this year
    });
  });

  describe('Form Submission', () => {
    let mockForm: jasmine.SpyObj<NgForm>;

    beforeEach(() => {
      mockForm = jasmine.createSpyObj('NgForm', [], { valid: true });
      component.customerData = {
        firstName: 'Juan',
        lastName: 'Pérez',
        email: 'juan@example.com',
        phone: '3001234567',
        documentType: 'CC',
        documentNumber: '12345678',
        address: 'Calle 123',
        city: 'Bogotá',
        birthDate: new Date('1990-01-01')
      };
      
      customerServiceSpy.validateEmail.and.returnValue(true);
      customerServiceSpy.validatePhone.and.returnValue(true);
      customerServiceSpy.validateDocumentNumber.and.returnValue(true);
    });

    it('should not submit if form is invalid', () => {
      mockForm = jasmine.createSpyObj('NgForm', [], { valid: false });
      
      component.onSubmit(mockForm);
      
      expect(customerServiceSpy.createCustomer).not.toHaveBeenCalled();
    });

    it('should not submit if already submitting', () => {
      component.isSubmitting = true;
      
      component.onSubmit(mockForm);
      
      expect(customerServiceSpy.createCustomer).not.toHaveBeenCalled();
    });

    it('should create customer and reservation successfully', fakeAsync(() => {
      const mockReservation = {
        id: 1,
        confirmationNumber: 'RF123456',
        customerId: 1,
        vehicleId: 1,
        startDate: new Date(),
        endDate: new Date(),
        totalPrice: 560000,
        status: ReservationStatus.Confirmed,
        createdAt: new Date()
      };

      customerServiceSpy.createCustomer.and.returnValue(of(mockCustomer));
      reservationServiceSpy.generateConfirmationNumber.and.returnValue('RF123456');
      reservationServiceSpy.createReservation.and.returnValue(of(mockReservation));
      
      component.onSubmit(mockForm);
      tick(); // Process the customer creation observable
      
      expect(customerServiceSpy.createCustomer).toHaveBeenCalledWith(component.customerData);
      expect(reservationServiceSpy.createReservation).toHaveBeenCalled();
      expect(component.showSuccessMessage).toBe(true);
      expect(component.confirmationNumber).toBe('RF123456');
      expect(component.isSubmitting).toBe(false);

      // Should navigate to confirmation after 3 seconds
      tick(3000);
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/booking/confirmation'], {
        queryParams: { confirmationNumber: 'RF123456' }
      });
    }));

    it('should handle customer creation error', () => {
      customerServiceSpy.createCustomer.and.returnValue(throwError(() => new Error('API Error')));
      spyOn(console, 'error');
      
      component.onSubmit(mockForm);
      
      expect(component.errorMessage).toContain('Error al crear el cliente');
      expect(component.isSubmitting).toBe(false);
      expect(console.error).toHaveBeenCalled();
    });

    it('should handle reservation creation error', () => {
      customerServiceSpy.createCustomer.and.returnValue(of(mockCustomer));
      reservationServiceSpy.generateConfirmationNumber.and.returnValue('RF123456');
      reservationServiceSpy.createReservation.and.returnValue(throwError(() => new Error('API Error')));
      spyOn(console, 'error');
      
      component.onSubmit(mockForm);
      
      expect(component.errorMessage).toContain('Error al crear la reserva');
      expect(component.isSubmitting).toBe(false);
      expect(console.error).toHaveBeenCalled();
    });
  });

  describe('Utility Methods', () => {
    it('should format price using vehicle service', () => {
      const price = 120000;
      vehicleServiceSpy.formatPrice.and.returnValue('$\u00A0120.000,00');
      
      const result = component.formatPrice(price);
      
      expect(vehicleServiceSpy.formatPrice).toHaveBeenCalledWith(price);
      expect(result).toBe('$\u00A0120.000,00');
    });

    it('should format phone number using customer service', () => {
      const phone = '3001234567';
      customerServiceSpy.formatPhoneNumber.and.returnValue('300 123 4567');
      component.customerData.phone = phone;
      
      component.formatPhoneNumber();
      
      expect(customerServiceSpy.formatPhoneNumber).toHaveBeenCalledWith(phone);
      expect(component.customerData.phone).toBe('300 123 4567');
    });

    it('should go back in browser history', () => {
      spyOn(window.history, 'back');
      
      component.goBack();
      
      expect(window.history.back).toHaveBeenCalled();
    });

    it('should navigate to confirmation page', () => {
      component.confirmationNumber = 'RF123456';
      
      component.navigateToConfirmation();
      
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/booking/confirmation'], {
        queryParams: { confirmationNumber: 'RF123456' }
      });
    });
  });

  describe('Reservation Data Creation', () => {
    it('should create correct reservation data', () => {
      component.bookingData = {
        vehicleId: 1,
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-07'),
        totalPrice: 560000
      };
      
      reservationServiceSpy.generateConfirmationNumber.and.returnValue('RF123456');
      reservationServiceSpy.createReservation.and.returnValue(of({
        id: 1,
        confirmationNumber: 'RF123456',
        customerId: 1,
        vehicleId: 1,
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-07'),
        totalPrice: 560000,
        status: ReservationStatus.Pending,
        createdAt: new Date()
      }));
      
      (component as any).createReservation(mockCustomer);
      
      const expectedReservationData: CreateReservation = {
        customerId: mockCustomer.id,
        vehicleId: 1,
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-07'),
        totalPrice: 560000,
        status: ReservationStatus.Pending,
        confirmationNumber: 'RF123456',
        notes: ''
      };
      
      expect(reservationServiceSpy.createReservation).toHaveBeenCalledWith(expectedReservationData);
    });
  });
});