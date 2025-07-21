import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiResponse } from '../../shared/models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly vehicleServiceUrl = 'http://localhost:5002/api';
  private readonly bookingServiceUrl = 'http://localhost:5257/api';

  constructor(private http: HttpClient) {}

  // Vehicle Service Endpoints
  getVehicles(): Observable<any[]> {
    return this.http.get<ApiResponse>(`${this.vehicleServiceUrl}/vehicles`)
      .pipe(
        map(response => response.data || []),
        catchError(this.handleError)
      );
  }

  // Test method to check API connectivity
  testVehicleAPI(): Observable<any> {
    console.log('Testing vehicle API connectivity...');
    return this.http.get<ApiResponse>(`${this.vehicleServiceUrl}/vehicles`)
      .pipe(
        map(response => {
          console.log('Vehicle API test successful:', response);
          return response;
        }),
        catchError(error => {
          console.error('Vehicle API test failed:', error);
          return this.handleError(error);
        })
      );
  }

  getVehicle(id: number): Observable<any> {
    return this.http.get<ApiResponse>(`${this.vehicleServiceUrl}/vehicles/${id}`)
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  checkVehicleAvailability(vehicleId: number, startDate: Date, endDate: Date): Observable<boolean> {
    const params = new HttpParams()
      .set('vehicleId', vehicleId.toString())
      .set('startDate', startDate.toISOString().split('T')[0])
      .set('endDate', endDate.toISOString().split('T')[0]);

    return this.http.get<ApiResponse<boolean>>(`${this.bookingServiceUrl}/reservations/check-availability`, { params })
      .pipe(
        map(response => response.data || false),
        catchError(this.handleError)
      );
  }

  getAvailableVehicles(startDate: Date, endDate: Date, type?: string): Observable<any[]> {
    let params = new HttpParams()
      .set('startDate', startDate.toISOString().split('T')[0])
      .set('endDate', endDate.toISOString().split('T')[0]);
    
    if (type) {
      params = params.set('type', type);
    }

    return this.http.get<ApiResponse>(`${this.vehicleServiceUrl}/vehicles/availability`, { params })
      .pipe(
        map(response => response.data || []),
        catchError(this.handleError)
      );
  }

  createVehicle(vehicle: any): Observable<any> {
    console.log('Creating vehicle with data:', vehicle);
    console.log('Request URL:', `${this.vehicleServiceUrl}/vehicles`);
    console.log('Request payload:', JSON.stringify(vehicle, null, 2));
    console.log('Payload type details:', {
      type: typeof vehicle.type,
      typeValue: vehicle.type,
      model: typeof vehicle.model,
      modelValue: vehicle.model,
      year: typeof vehicle.year,
      yearValue: vehicle.year,
      pricePerDay: typeof vehicle.pricePerDay,
      pricePerDayValue: vehicle.pricePerDay
    });
    
    const headers = {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    };
    
    // Convert to PascalCase for C# backend
    const backendPayload = {
      Type: vehicle.type,
      Model: vehicle.model,
      Year: vehicle.year,
      PricePerDay: vehicle.pricePerDay
    };
    console.log('Backend payload with PascalCase:', JSON.stringify(backendPayload, null, 2));
    
    return this.http.post<ApiResponse>(`${this.vehicleServiceUrl}/vehicles`, backendPayload, { headers })
      .pipe(
        map(response => {
          console.log('Create vehicle response:', response);
          return response.data;
        }),
        catchError(this.handleError)
      );
  }

  updateVehicle(id: number, vehicle: any): Observable<any> {
    return this.http.put<ApiResponse>(`${this.vehicleServiceUrl}/vehicles/${id}`, vehicle)
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  deleteVehicle(id: number): Observable<void> {
    return this.http.delete<ApiResponse>(`${this.vehicleServiceUrl}/vehicles/${id}`)
      .pipe(
        map(() => void 0),
        catchError(this.handleError)
      );
  }

  // Booking Service Endpoints
  createReservation(reservation: any): Observable<any> {
    console.log('Creating reservation with data:', reservation);
    console.log('Request URL:', `${this.bookingServiceUrl}/reservations`);
    
    // Convert to PascalCase format for C# backend
    const backendPayload = {
      VehicleId: reservation.vehicleId,
      StartDate: new Date(reservation.startDate).toISOString(),
      EndDate: new Date(reservation.endDate).toISOString(),
      CustomerInfo: {
        Name: reservation.customerInfo.name,
        Email: reservation.customerInfo.email,
        Phone: reservation.customerInfo.phone,
        DocumentNumber: reservation.customerInfo.documentNumber
      }
    };
    
    console.log('Backend payload with PascalCase:', JSON.stringify(backendPayload, null, 2));
    
    const headers = {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    };
    
    return this.http.post<ApiResponse>(`${this.bookingServiceUrl}/reservations`, backendPayload, { headers })
      .pipe(
        map(response => {
          console.log('Create reservation response:', response);
          return response.data;
        }),
        catchError(this.handleError)
      );
  }

  getReservation(id: number): Observable<any> {
    return this.http.get<ApiResponse>(`${this.bookingServiceUrl}/reservations/${id}`)
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  getReservationByConfirmation(confirmationNumber: string): Observable<any> {
    return this.http.get<ApiResponse>(`${this.bookingServiceUrl}/reservations/by-confirmation/${confirmationNumber}`)
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  searchReservations(params: { 
    startDate?: Date; 
    endDate?: Date; 
    status?: string; 
    customerId?: number; 
    vehicleId?: number;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<any[]> {
    let httpParams = new HttpParams();
    
    if (params.startDate) {
      httpParams = httpParams.set('startDate', params.startDate.toISOString());
    }
    if (params.endDate) {
      httpParams = httpParams.set('endDate', params.endDate.toISOString());
    }
    if (params.status) {
      httpParams = httpParams.set('status', params.status);
    }
    if (params.customerId) {
      httpParams = httpParams.set('customerId', params.customerId.toString());
    }
    if (params.vehicleId) {
      httpParams = httpParams.set('vehicleId', params.vehicleId.toString());
    }
    if (params.pageNumber && params.pageNumber > 0) {
      httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    }
    if (params.pageSize && params.pageSize > 0) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }
    
    return this.http.get<ApiResponse>(`${this.bookingServiceUrl}/reservations`, { params: httpParams })
      .pipe(
        map(response => response.data || []),
        catchError(this.handleError)
      );
  }

  cancelReservation(id: number, reason: string): Observable<any> {
    return this.http.post<ApiResponse>(`${this.bookingServiceUrl}/reservations/${id}/cancel`, { reason })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  confirmReservation(id: number): Observable<any> {
    return this.http.post<ApiResponse>(`${this.bookingServiceUrl}/reservations/${id}/confirm`, {})
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  // Customer Service Endpoints
  createCustomer(customer: any): Observable<any> {
    return this.http.post<ApiResponse>(`${this.bookingServiceUrl}/customers`, customer)
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  getCustomer(id: number): Observable<any> {
    return this.http.get<ApiResponse>(`${this.bookingServiceUrl}/customers/${id}`)
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  getCustomerByEmail(email: string): Observable<any> {
    return this.http.get<ApiResponse>(`${this.bookingServiceUrl}/customers/by-email/${email}`)
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  getCustomerHistory(customerId: number): Observable<any[]> {
    return this.http.get<ApiResponse>(`${this.bookingServiceUrl}/customers/${customerId}/history`)
      .pipe(
        map(response => response.data || []),
        catchError(this.handleError)
      );
  }

  private handleError(error: any): Observable<never> {
    console.error('API Error:', error);
    console.error('Error Status:', error.status);
    console.error('Error Body:', error.error);
    console.error('Error Message:', error.message);
    console.error('Full error object JSON:', JSON.stringify(error.error, null, 2));
    
    // Log detailed validation errors
    if (error.error && error.error.errors) {
      console.error('Detailed validation errors:', error.error.errors);
      console.error('Full error object:', JSON.stringify(error.error.errors, null, 2));
      Object.entries(error.error.errors).forEach(([field, messages]) => {
        console.error(`Validation error for ${field}:`, messages);
      });
    } else {
      console.error('No detailed validation errors found in response');
    }
    
    let errorMessage = 'An error occurred';
    
    if (error.error?.message) {
      errorMessage = error.error.message;
    } else if (error.error?.errors && error.error.errors.length > 0) {
      errorMessage = error.error.errors.join(', ');
    } else if (error.error && typeof error.error === 'string') {
      errorMessage = error.error;
    } else if (error.message) {
      errorMessage = error.message;
    }
    
    return throwError(() => new Error(errorMessage));
  }
}