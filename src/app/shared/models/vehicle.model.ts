export enum VehicleType {
  SUV = 0,
  Sedan = 1,
  Compact = 2
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