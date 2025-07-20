import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiResponse } from '../../shared/models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly vehicleServiceUrl = 'http://localhost:5001/api';
  private readonly bookingServiceUrl = 'http://localhost:5002/api';

  constructor(private http: HttpClient) {}

  // Vehicle Service Endpoints
  getVehicles(): Observable<any[]> {
    return this.http.get<ApiResponse>(`${this.vehicleServiceUrl}/vehicles`)
      .pipe(
        map(response => response.data || []),
        catchError(this.handleError)
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
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<ApiResponse<boolean>>(`${this.bookingServiceUrl}/reservations/check-availability`, { params })
      .pipe(
        map(response => response.data || false),
        catchError(this.handleError)
      );
  }

  getAvailableVehicles(startDate: Date, endDate: Date, type?: string): Observable<any[]> {
    let params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    
    if (type) {
      params = params.set('type', type);
    }

    return this.http.get<ApiResponse>(`${this.vehicleServiceUrl}/vehicles/availability`, { params })
      .pipe(
        map(response => response.data || []),
        catchError(this.handleError)
      );
  }

  // Booking Service Endpoints
  createReservation(reservation: any): Observable<any> {
    return this.http.post<ApiResponse>(`${this.bookingServiceUrl}/reservations`, reservation)
      .pipe(
        map(response => response.data),
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
    let errorMessage = 'An error occurred';
    
    if (error.error?.message) {
      errorMessage = error.error.message;
    } else if (error.error?.errors && error.error.errors.length > 0) {
      errorMessage = error.error.errors.join(', ');
    } else if (error.message) {
      errorMessage = error.message;
    }
    
    return throwError(() => new Error(errorMessage));
  }
}