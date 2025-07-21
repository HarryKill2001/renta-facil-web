# Frontend Structure - RentaFácil Angular Application

## Overview
The RentaFácil frontend is built with Angular 17+ using standalone components and modern Angular best practices. The application provides a responsive, user-friendly interface for vehicle search, reservation management, and customer history viewing with proper API integration to the .NET microservices backend.

## Project Structure
```
renta-facil-web/
├── src/app/                         # Angular 17+ application
│   ├── core/                        # Core functionality (singleton services)
│   │   ├── guards/                  # Route guards (future auth)
│   │   ├── interceptors/            # HTTP interceptors (global error handling)
│   │   ├── services/                # Core services
│   │   │   ├── api.service.ts       # Centralized API communication
│   │   │   ├── reservation.service.ts # Reservation operations
│   │   │   └── vehicle.service.ts   # Vehicle operations
│   │   └── models/                  # TypeScript interfaces/types (currently unused)
│   ├── shared/                      # Shared components and utilities
│   │   ├── components/              # Reusable UI components (planned)
│   │   ├── models/                  # Domain models and DTOs
│   │   │   ├── customer.model.ts    # Customer interfaces
│   │   │   ├── vehicle.model.ts     # Vehicle interfaces
│   │   │   └── reservation.model.ts # Reservation interfaces (planned)
│   │   ├── services/                # Shared utility services (planned)
│   │   └── validators/              # Custom form validators (planned)
│   ├── features/                    # Feature modules (planned - not yet implemented)
│   │   ├── vehicle-search/          # Vehicle search functionality (future)
│   │   ├── reservation/             # Reservation management (future)
│   │   └── customer-history/        # Customer history viewing (future)
│   ├── app.component.html           # Root template with reservation form
│   ├── app.component.ts             # Root component with reservation logic
│   ├── app.component.scss           # Root component styles
│   ├── app.config.ts                # Standalone component configuration
│   └── app.routes.ts                # Application routing (basic)
├── assets/                          # Static assets
├── environments/                    # Environment configurations (not configured)
├── styles.scss                      # Global styles
├── angular.json                     # Angular CLI configuration
├── package.json                     # Dependencies
└── tsconfig.json                    # TypeScript configuration
```

## Core Architecture

### Application Configuration
```typescript
// app.config.ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([apiInterceptor, errorInterceptor])),
    importProvidersFrom(ReactiveFormsModule),
    { provide: 'API_BASE_URL', useValue: environment.apiBaseUrl }
  ]
};
```

### Routing Configuration
```typescript
// app.routes.ts
export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { 
    path: 'dashboard', 
    loadComponent: () => import('./features/dashboard/dashboard.component')
      .then(m => m.DashboardComponent)
  },
  { 
    path: 'search', 
    loadComponent: () => import('./features/vehicle-search/vehicle-search.component')
      .then(m => m.VehicleSearchComponent)
  },
  { 
    path: 'reservation', 
    loadComponent: () => import('./features/reservation/reservation.component')
      .then(m => m.ReservationComponent)
  },
  { 
    path: 'history/:customerId', 
    loadComponent: () => import('./features/customer-history/customer-history.component')
      .then(m => m.CustomerHistoryComponent)
  },
  { path: '**', redirectTo: '/dashboard' }
];
```

## Current Implementation

### Centralized API Service
The current implementation uses a centralized `ApiService` that handles all HTTP communications:

```typescript
// core/services/api.service.ts
@Injectable({ providedIn: 'root' })
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

    return this.http.get<ApiResponse>(`${this.vehicleServiceUrl}/vehicles/availability`, { params })
      .pipe(
        map(response => response.success),
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

  private handleError = (error: any): Observable<never> => {
    console.error('API Error:', error);
    return throwError(() => error);
  };
}
```

### 2. Reservation Service
```typescript
// core/services/reservation.service.ts
@Injectable({ providedIn: 'root' })
export class ReservationService {
  private readonly apiUrl = `${this.baseUrl}/api/reservations`;

  constructor(
    private http: HttpClient,
    @Inject('API_BASE_URL') private baseUrl: string
  ) {}

  createReservation(reservation: CreateReservationRequest): Observable<Reservation> {
    return this.http.post<ApiResponse<Reservation>>(this.apiUrl, reservation)
      .pipe(map(response => response.data));
  }

  getReservations(params?: ReservationQueryParams): Observable<PaginatedResponse<Reservation>> {
    const httpParams = this.buildQueryParams(params);
    return this.http.get<ApiResponse<PaginatedResponse<Reservation>>>(this.apiUrl, { params: httpParams })
      .pipe(map(response => response.data));
  }

  getReservationById(id: number): Observable<ReservationDetail> {
    return this.http.get<ApiResponse<ReservationDetail>>(`${this.apiUrl}/${id}`)
      .pipe(map(response => response.data));
  }
}
```

### 3. Customer Service
```typescript
// core/services/customer.service.ts
@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly apiUrl = `${this.baseUrl}/api/customers`;

  constructor(
    private http: HttpClient,
    @Inject('API_BASE_URL') private baseUrl: string
  ) {}

  createCustomer(customer: CreateCustomerRequest): Observable<Customer> {
    return this.http.post<ApiResponse<Customer>>(this.apiUrl, customer)
      .pipe(map(response => response.data));
  }

  getCustomerHistory(customerId: number): Observable<CustomerHistory> {
    return this.http.get<ApiResponse<CustomerHistory>>(`${this.apiUrl}/${customerId}/history`)
      .pipe(map(response => response.data));
  }
}
```

## Data Models

### TypeScript Interfaces
```typescript
// core/models/vehicle.model.ts
export interface Vehicle {
  id: number;
  type: string;
  model: string;
  year: number;
  pricePerDay: number;
  available: boolean;
  createdAt: Date;
}

export interface VehicleSearchParams {
  startDate: Date;
  endDate: Date;
  type?: string;
}

// core/models/reservation.model.ts
export interface Reservation {
  id: number;
  customerId: number;
  vehicleId: number;
  startDate: Date;
  endDate: Date;
  totalPrice: number;
  status: ReservationStatus;
  confirmationNumber: string;
  createdAt: Date;
}

export interface CreateReservationRequest {
  customerId?: number;
  vehicleId: number;
  startDate: Date;
  endDate: Date;
  customerInfo?: CustomerInfo;
}

export interface CustomerInfo {
  name: string;
  email: string;
  phone: string;
  documentNumber: string;
}

export enum ReservationStatus {
  Confirmed = 'Confirmed',
  Cancelled = 'Cancelled',
  Completed = 'Completed'
}

// core/models/customer.model.ts
export interface Customer {
  id: number;
  name: string;
  email: string;
  phone: string;
  documentNumber: string;
  createdAt: Date;
}

export interface CustomerHistory {
  customer: Customer;
  reservations: ReservationSummary[];
}

// core/models/common.model.ts
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors?: string[];
}

export interface PaginatedResponse<T> {
  items: T[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}
```

## Feature Components

### 1. Vehicle Search Component
```typescript
// features/vehicle-search/vehicle-search.component.ts
@Component({
  selector: 'app-vehicle-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDatepickerModule, MatFormFieldModule],
  template: `
    <div class="search-container">
      <h2>Buscar Vehículos Disponibles</h2>
      
      <form [formGroup]="searchForm" (ngSubmit)="onSearch()">
        <div class="form-row">
          <mat-form-field>
            <mat-label>Fecha de inicio</mat-label>
            <input matInput [matDatepicker]="startPicker" formControlName="startDate">
            <mat-datepicker-toggle matSuffix [for]="startPicker"></mat-datepicker-toggle>
            <mat-datepicker #startPicker></mat-datepicker>
          </mat-form-field>

          <mat-form-field>
            <mat-label>Fecha de fin</mat-label>
            <input matInput [matDatepicker]="endPicker" formControlName="endDate">
            <mat-datepicker-toggle matSuffix [for]="endPicker"></mat-datepicker-toggle>
            <mat-datepicker #endPicker></mat-datepicker>
          </mat-form-field>

          <mat-form-field>
            <mat-label>Tipo de vehículo</mat-label>
            <mat-select formControlName="vehicleType">
              <mat-option value="">Todos</mat-option>
              <mat-option value="SUV">SUV</mat-option>
              <mat-option value="Sedan">Sedán</mat-option>
              <mat-option value="Compact">Compacto</mat-option>
            </mat-select>
          </mat-form-field>

          <button mat-raised-button color="primary" type="submit" [disabled]="searchForm.invalid">
            Buscar
          </button>
        </div>
      </form>

      <div class="results-container" *ngIf="vehicles$ | async as vehicles">
        <h3>Vehículos Disponibles</h3>
        <div class="vehicle-grid">
          <div *ngFor="let vehicle of vehicles" class="vehicle-card">
            <h4>{{ vehicle.model }}</h4>
            <p>Tipo: {{ vehicle.type }}</p>
            <p>Año: {{ vehicle.year }}</p>
            <p>Precio por día: {{ vehicle.pricePerDay | currency:'COP' }}</p>
            <button mat-raised-button color="accent" 
                    (click)="selectVehicle(vehicle)">
              Seleccionar
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrl: './vehicle-search.component.scss'
})
export class VehicleSearchComponent implements OnInit {
  searchForm: FormGroup;
  vehicles$ = new BehaviorSubject<Vehicle[]>([]);
  loading = false;

  constructor(
    private fb: FormBuilder,
    private vehicleService: VehicleService,
    private router: Router
  ) {
    this.searchForm = this.fb.group({
      startDate: ['', [Validators.required, this.dateValidator]],
      endDate: ['', [Validators.required, this.dateValidator]],
      vehicleType: ['']
    });
  }

  ngOnInit(): void {
    this.searchForm.get('startDate')?.valueChanges.subscribe(() => {
      this.validateDateRange();
    });
  }

  onSearch(): void {
    if (this.searchForm.valid) {
      this.loading = true;
      const searchParams: VehicleSearchParams = {
        startDate: this.searchForm.value.startDate,
        endDate: this.searchForm.value.endDate,
        type: this.searchForm.value.vehicleType
      };

      this.vehicleService.getAvailableVehicles(searchParams)
        .pipe(finalize(() => this.loading = false))
        .subscribe({
          next: (vehicles) => this.vehicles$.next(vehicles),
          error: (error) => this.handleError(error)
        });
    }
  }

  selectVehicle(vehicle: Vehicle): void {
    const navigationExtras: NavigationExtras = {
      state: {
        vehicle,
        searchParams: this.searchForm.value
      }
    };
    this.router.navigate(['/reservation'], navigationExtras);
  }

  private dateValidator = (control: AbstractControl): ValidationErrors | null => {
    const date = new Date(control.value);
    return date < new Date() ? { pastDate: true } : null;
  };

  private validateDateRange(): void {
    const startDate = this.searchForm.get('startDate')?.value;
    const endDate = this.searchForm.get('endDate')?.value;
    
    if (startDate && endDate && startDate >= endDate) {
      this.searchForm.get('endDate')?.setErrors({ invalidRange: true });
    }
  }

  private handleError(error: any): void {
    // Handle error appropriately
    console.error('Search error:', error);
  }
}
```

### 2. Reservation Component
```typescript
// features/reservation/reservation.component.ts
@Component({
  selector: 'app-reservation',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatStepperModule],
  template: `
    <div class="reservation-container">
      <h2>Crear Reserva</h2>
      
      <mat-stepper [linear]="true" #stepper>
        <!-- Vehicle Summary Step -->
        <mat-step [stepControl]="vehicleForm">
          <ng-template matStepLabel>Vehículo Seleccionado</ng-template>
          <div class="vehicle-summary" *ngIf="selectedVehicle">
            <h3>{{ selectedVehicle.model }}</h3>
            <p>Tipo: {{ selectedVehicle.type }}</p>
            <p>Precio por día: {{ selectedVehicle.pricePerDay | currency:'COP' }}</p>
            <p>Días: {{ totalDays }}</p>
            <p><strong>Total: {{ totalPrice | currency:'COP' }}</strong></p>
          </div>
          <div>
            <button mat-button matStepperNext>Siguiente</button>
          </div>
        </mat-step>

        <!-- Customer Information Step -->
        <mat-step [stepControl]="customerForm">
          <ng-template matStepLabel>Información del Cliente</ng-template>
          <form [formGroup]="customerForm">
            <mat-form-field>
              <mat-label>Nombre completo</mat-label>
              <input matInput formControlName="name" required>
            </mat-form-field>

            <mat-form-field>
              <mat-label>Email</mat-label>
              <input matInput type="email" formControlName="email" required>
            </mat-form-field>

            <mat-form-field>
              <mat-label>Teléfono</mat-label>
              <input matInput formControlName="phone" required>
            </mat-form-field>

            <mat-form-field>
              <mat-label>Número de documento</mat-label>
              <input matInput formControlName="documentNumber" required>
            </mat-form-field>
          </form>
          <div>
            <button mat-button matStepperPrevious>Anterior</button>
            <button mat-button matStepperNext [disabled]="customerForm.invalid">Siguiente</button>
          </div>
        </mat-step>

        <!-- Confirmation Step -->
        <mat-step>
          <ng-template matStepLabel>Confirmación</ng-template>
          <div class="confirmation-summary">
            <h3>Resumen de la Reserva</h3>
            <div class="summary-details">
              <p><strong>Vehículo:</strong> {{ selectedVehicle?.model }}</p>
              <p><strong>Cliente:</strong> {{ customerForm.value.name }}</p>
              <p><strong>Email:</strong> {{ customerForm.value.email }}</p>
              <p><strong>Fechas:</strong> {{ searchParams?.startDate | date }} - {{ searchParams?.endDate | date }}</p>
              <p><strong>Total:</strong> {{ totalPrice | currency:'COP' }}</p>
            </div>
          </div>
          <div>
            <button mat-button matStepperPrevious>Anterior</button>
            <button mat-raised-button color="primary" 
                    (click)="createReservation()" 
                    [disabled]="loading">
              {{ loading ? 'Procesando...' : 'Confirmar Reserva' }}
            </button>
          </div>
        </mat-step>
      </mat-stepper>
    </div>
  `,
  styleUrl: './reservation.component.scss'
})
export class ReservationComponent implements OnInit {
  vehicleForm: FormGroup;
  customerForm: FormGroup;
  selectedVehicle?: Vehicle;
  searchParams?: any;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private reservationService: ReservationService,
    private router: Router
  ) {
    this.vehicleForm = this.fb.group({});
    this.customerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^\+?[\d\s-()]+$/)]],
      documentNumber: ['', [Validators.required]]
    });

    // Get navigation state
    const navigation = this.router.getCurrentNavigation();
    if (navigation?.extras.state) {
      this.selectedVehicle = navigation.extras.state['vehicle'];
      this.searchParams = navigation.extras.state['searchParams'];
    }
  }

  ngOnInit(): void {
    if (!this.selectedVehicle) {
      this.router.navigate(['/search']);
    }
  }

  get totalDays(): number {
    if (!this.searchParams) return 0;
    const start = new Date(this.searchParams.startDate);
    const end = new Date(this.searchParams.endDate);
    return Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24));
  }

  get totalPrice(): number {
    return (this.selectedVehicle?.pricePerDay || 0) * this.totalDays;
  }

  createReservation(): void {
    if (this.customerForm.valid && this.selectedVehicle) {
      this.loading = true;
      
      const request: CreateReservationRequest = {
        vehicleId: this.selectedVehicle.id,
        startDate: new Date(this.searchParams.startDate),
        endDate: new Date(this.searchParams.endDate),
        customerInfo: this.customerForm.value
      };

      this.reservationService.createReservation(request)
        .pipe(finalize(() => this.loading = false))
        .subscribe({
          next: (reservation) => {
            this.router.navigate(['/confirmation'], { 
              state: { reservation } 
            });
          },
          error: (error) => this.handleError(error)
        });
    }
  }

  private handleError(error: any): void {
    // Handle error appropriately
    console.error('Reservation error:', error);
  }
}
```

### 3. Customer History Component
```typescript
// features/customer-history/customer-history.component.ts
@Component({
  selector: 'app-customer-history',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule],
  template: `
    <div class="history-container">
      <h2>Historial de Reservas</h2>
      
      <div class="customer-info" *ngIf="customerHistory$ | async as history">
        <h3>{{ history.customer.name }}</h3>
        <p>Email: {{ history.customer.email }}</p>
        <p>Teléfono: {{ history.customer.phone }}</p>
      </div>

      <div class="reservations-table">
        <table mat-table [dataSource]="dataSource" class="mat-elevation-z8">
          <ng-container matColumnDef="confirmationNumber">
            <th mat-header-cell *matHeaderCellDef>Confirmación</th>
            <td mat-cell *matCellDef="let reservation">{{ reservation.confirmationNumber }}</td>
          </ng-container>

          <ng-container matColumnDef="vehicleModel">
            <th mat-header-cell *matHeaderCellDef>Vehículo</th>
            <td mat-cell *matCellDef="let reservation">{{ reservation.vehicleModel }}</td>
          </ng-container>

          <ng-container matColumnDef="dates">
            <th mat-header-cell *matHeaderCellDef>Fechas</th>
            <td mat-cell *matCellDef="let reservation">
              {{ reservation.startDate | date }} - {{ reservation.endDate | date }}
            </td>
          </ng-container>

          <ng-container matColumnDef="totalPrice">
            <th mat-header-cell *matHeaderCellDef>Total</th>
            <td mat-cell *matCellDef="let reservation">{{ reservation.totalPrice | currency:'COP' }}</td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Estado</th>
            <td mat-cell *matCellDef="let reservation">
              <span [class]="'status-' + reservation.status.toLowerCase()">
                {{ reservation.status }}
              </span>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>

        <mat-paginator [pageSizeOptions]="[5, 10, 20]" showFirstLastButtons></mat-paginator>
      </div>
    </div>
  `,
  styleUrl: './customer-history.component.scss'
})
export class CustomerHistoryComponent implements OnInit {
  customerHistory$ = new BehaviorSubject<CustomerHistory | null>(null);
  dataSource = new MatTableDataSource<ReservationSummary>();
  displayedColumns: string[] = ['confirmationNumber', 'vehicleModel', 'dates', 'totalPrice', 'status'];

  constructor(
    private route: ActivatedRoute,
    private customerService: CustomerService
  ) {}

  ngOnInit(): void {
    const customerId = Number(this.route.snapshot.paramMap.get('customerId'));
    if (customerId) {
      this.loadCustomerHistory(customerId);
    }
  }

  private loadCustomerHistory(customerId: number): void {
    this.customerService.getCustomerHistory(customerId)
      .subscribe({
        next: (history) => {
          this.customerHistory$.next(history);
          this.dataSource.data = history.reservations;
        },
        error: (error) => this.handleError(error)
      });
  }

  private handleError(error: any): void {
    console.error('History loading error:', error);
  }
}
```

## Shared Components

### Loading Spinner
```typescript
// shared/components/loading-spinner/loading-spinner.component.ts
@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  template: `
    <div class="spinner-container">
      <mat-spinner [diameter]="diameter"></mat-spinner>
      <p *ngIf="message">{{ message }}</p>
    </div>
  `,
  styleUrl: './loading-spinner.component.scss'
})
export class LoadingSpinnerComponent {
  @Input() diameter = 50;
  @Input() message = '';
}
```

### Error Display
```typescript
// shared/components/error-display/error-display.component.ts
@Component({
  selector: 'app-error-display',
  standalone: true,
  template: `
    <div class="error-container" *ngIf="error">
      <mat-icon>error</mat-icon>
      <h3>{{ error.title || 'Error' }}</h3>
      <p>{{ error.message }}</p>
      <button mat-button (click)="onRetry()" *ngIf="showRetry">
        Reintentar
      </button>
    </div>
  `,
  styleUrl: './error-display.component.scss'
})
export class ErrorDisplayComponent {
  @Input() error: any;
  @Input() showRetry = false;
  @Output() retry = new EventEmitter<void>();

  onRetry(): void {
    this.retry.emit();
  }
}
```

## Environment Configuration

### Development Environment
```typescript
// environments/environment.ts
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5257',
  vehicleServiceUrl: 'http://localhost:5002',
  enableLogging: true
};
```

### Production Environment
```typescript
// environments/environment.prod.ts
export const environment = {
  production: true,
  apiBaseUrl: 'https://api.rentafacil.com',
  vehicleServiceUrl: 'https://vehicles.rentafacil.com',
  enableLogging: false
};
```

## Development Proxy Configuration
```json
// proxy.conf.json - Updated for correct service ports
{
  "/api/vehicles/*": {
    "target": "http://localhost:5002",
    "secure": false,
    "changeOrigin": true,
    "logLevel": "debug"
  },
  "/api/*": {
    "target": "http://localhost:5257",
    "secure": false,
    "changeOrigin": true,
    "logLevel": "debug"
  }
}
```

## Styling and Theming

### Angular Material Theme
```scss
// styles/theme.scss
@use '@angular/material' as mat;

$primary-palette: mat.define-palette(mat.$blue-palette);
$accent-palette: mat.define-palette(mat.$orange-palette);
$warn-palette: mat.define-palette(mat.$red-palette);

$theme: mat.define-light-theme((
  color: (
    primary: $primary-palette,
    accent: $accent-palette,
    warn: $warn-palette,
  )
));

@include mat.all-component-themes($theme);
```

### Global Styles
```scss
// styles/global.scss
.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.form-row {
  display: flex;
  gap: 16px;
  margin-bottom: 16px;
  
  mat-form-field {
    flex: 1;
  }
}

.vehicle-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  margin-top: 20px;
}

.vehicle-card {
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 16px;
  text-align: center;
  
  &:hover {
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  }
}
```

This frontend structure provides a solid foundation for the RentaFácil Angular application with modern Angular practices, responsive design, and clear separation of concerns.