export interface Customer {
  id: number;
  name: string;
  email: string;
  phone: string;
  documentNumber: string;
  createdAt: string;
}

export interface CreateCustomer {
  name: string;
  email: string;
  phone: string;
  documentNumber: string;
}

export interface UpdateCustomer {
  name?: string;
  phone?: string;
}