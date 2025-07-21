import { Routes } from '@angular/router';
import { AdminGuard } from './core/guards';

export const routes: Routes = [
  // Home route
  {
    path: '',
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent),
    title: 'RentaFácil - Alquiler de Vehículos'
  },
  
  // Vehicle routes
  {
    path: 'vehicles',
    loadComponent: () => import('./features/vehicle-search/vehicle-search.component').then(m => m.VehicleSearchComponent),
    title: 'Buscar Vehículos - RentaFácil'
  },
  {
    path: 'vehicles/:id',
    loadComponent: () => import('./features/vehicle-details/vehicle-details.component').then(m => m.VehicleDetailsComponent),
    title: 'Detalles del Vehículo - RentaFácil'
  },
  
  // Booking routes
  {
    path: 'booking',
    loadComponent: () => import('./features/booking/booking-form.component').then(m => m.BookingFormComponent),
    title: 'Completar Reserva - RentaFácil'
  },
  {
    path: 'booking/confirmation',
    loadComponent: () => import('./features/booking/booking-confirmation.component').then(m => m.BookingConfirmationComponent),
    title: 'Confirmación de Reserva - RentaFácil'
  },
  
  // Information pages
  {
    path: 'about',
    loadComponent: () => import('./features/about/about.component').then(m => m.AboutComponent),
    title: 'Nosotros - RentaFácil'
  },
  {
    path: 'contact',
    loadComponent: () => import('./features/contact/contact.component').then(m => m.ContactComponent),
    title: 'Contacto - RentaFácil'
  },
  {
    path: 'reservation-search',
    loadComponent: () => import('./features/reservation-search/reservation-search.component').then(m => m.ReservationSearchComponent),
    title: 'Buscar Reserva - RentaFácil'
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent),
    title: 'Iniciar Sesión - RentaFácil'
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register.component').then(m => m.RegisterComponent),
    title: 'Registro - RentaFácil'
  },
  
  // Admin routes (protected)
  {
    path: 'admin',
    loadComponent: () => import('./features/admin/admin-dashboard.component').then(m => m.AdminDashboardComponent),
    title: 'Panel de Administración - RentaFácil',
    canActivate: [AdminGuard]
  },
  {
    path: 'admin/vehicles',
    loadComponent: () => import('./features/admin/vehicle-management/vehicle-management.component').then(m => m.VehicleManagementComponent),
    title: 'Gestión de Vehículos - RentaFácil',
    canActivate: [AdminGuard]
  },
  {
    path: 'admin/vehicles/new',
    loadComponent: () => import('./features/admin/vehicle-form/vehicle-form.component').then(m => m.VehicleFormComponent),
    title: 'Nuevo Vehículo - RentaFácil',
    canActivate: [AdminGuard]
  },
  {
    path: 'admin/vehicles/:id',
    loadComponent: () => import('./features/admin/vehicle-form/vehicle-form.component').then(m => m.VehicleFormComponent),
    title: 'Editar Vehículo - RentaFácil',
    canActivate: [AdminGuard]
  },
  {
    path: 'admin/vehicles/:id/edit',
    loadComponent: () => import('./features/admin/vehicle-form/vehicle-form.component').then(m => m.VehicleFormComponent),
    title: 'Editar Vehículo - RentaFácil',
    canActivate: [AdminGuard]
  },
  
  // Error pages
  {
    path: 'error',
    loadComponent: () => import('./features/error/error-page.component').then(m => m.ErrorPageComponent),
    title: 'Error - RentaFácil'
  },
  {
    path: 'maintenance',
    loadComponent: () => import('./features/error/maintenance.component').then(m => m.MaintenanceComponent),
    title: 'Mantenimiento - RentaFácil'
  },
  
  // 404 - Must be last route
  {
    path: '**',
    loadComponent: () => import('./features/error/not-found.component').then(m => m.NotFoundComponent),
    title: 'Página no encontrada - RentaFácil'
  }
];
