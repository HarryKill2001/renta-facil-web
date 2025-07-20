import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Customer, CreateCustomer, Reservation } from '../../shared/models';

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  constructor(private apiService: ApiService) {}

  createCustomer(customer: CreateCustomer): Observable<Customer> {
    return this.apiService.createCustomer(customer);
  }

  getCustomerById(id: number): Observable<Customer> {
    return this.apiService.getCustomer(id);
  }

  getCustomerByEmail(email: string): Observable<Customer> {
    return this.apiService.getCustomerByEmail(email);
  }

  getCustomerHistory(customerId: number): Observable<Reservation[]> {
    return this.apiService.getCustomerHistory(customerId);
  }

  validateEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  validatePhone(phone: string): boolean {
    // Colombian phone number format
    const phoneRegex = /^\+?57\s?[39]\d{2}\s?\d{3}\s?\d{4}$/;
    return phoneRegex.test(phone.replace(/\s/g, ''));
  }

  validateDocumentNumber(documentNumber: string): boolean {
    // Basic validation for Colombian document numbers
    const docRegex = /^\d{6,12}$/;
    return docRegex.test(documentNumber);
  }

  formatPhoneNumber(phone: string): string {
    // Format Colombian phone number
    const cleaned = phone.replace(/\D/g, '');
    if (cleaned.length === 10) {
      return `+57 ${cleaned.substring(0, 3)} ${cleaned.substring(3, 6)} ${cleaned.substring(6)}`;
    }
    return phone;
  }
}