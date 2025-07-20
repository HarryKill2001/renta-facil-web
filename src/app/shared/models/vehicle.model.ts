export enum VehicleType {
  SUV = 'SUV',
  Sedan = 'Sedan',
  Compact = 'Compact'
}

export interface Vehicle {
  id: number;
  type: VehicleType;
  model: string;
  year: number;
  pricePerDay: number;
  available: boolean;
  createdAt: Date;
}

export interface CreateVehicle {
  type: VehicleType;
  model: string;
  year: number;
  pricePerDay: number;
}

export interface UpdateVehicle {
  pricePerDay?: number;
  available?: boolean;
}

export interface VehicleAvailabilityRequest {
  startDate: Date;
  endDate: Date;
  type?: VehicleType;
}