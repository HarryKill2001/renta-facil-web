import { Customer } from './customer.model';
import { Vehicle } from './vehicle.model';

export enum ReservationStatus {
  Pending = 'Pending',
  Confirmed = 'Confirmed',
  Cancelled = 'Cancelled',
  Completed = 'Completed'
}

export interface Reservation {
  id: number;
  confirmationNumber: string;
  vehicleId: number;
  customerId: number;
  startDate: Date;
  endDate: Date;
  totalPrice: number;
  status: ReservationStatus;
  createdAt: Date;
  vehicle?: Vehicle;
  customer?: Customer;
}

export interface CreateReservation {
  vehicleId: number;
  startDate: Date;
  endDate: Date;
  customerInfo: {
    name: string;
    email: string;
    phone: string;
    documentNumber: string;
  };
}

export interface ReservationSearch {
  startDate?: Date;
  endDate?: Date;
  status?: ReservationStatus;
  customerId?: number;
  vehicleId?: number;
  pageNumber?: number;
  pageSize?: number;
}

export interface CancelReservation {
  reason: string;
}