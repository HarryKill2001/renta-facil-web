import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  searchData = {
    startDate: '',
    endDate: '',
    vehicleType: ''
  };

  featuredVehicles = [
    {
      id: 1,
      model: 'Toyota RAV4',
      type: 'SUV',
      year: 2023,
      pricePerDay: 85000,
      image: 'suv-placeholder'
    },
    {
      id: 2,
      model: 'Nissan Sentra',
      type: 'Sedán',
      year: 2022,
      pricePerDay: 65000,
      image: 'sedan-placeholder'
    },
    {
      id: 3,
      model: 'Chevrolet Spark',
      type: 'Compacto',
      year: 2023,
      pricePerDay: 45000,
      image: 'compact-placeholder'
    }
  ];

  features = [
    {
      icon: '🚗',
      title: 'Amplia Flota',
      description: 'Más de 100 vehículos de diferentes categorías para satisfacer tus necesidades'
    },
    {
      icon: '💰',
      title: 'Mejores Precios',
      description: 'Tarifas competitivas y transparentes sin costos ocultos'
    },
    {
      icon: '🔒',
      title: 'Seguro Incluido',
      description: 'Todos nuestros vehículos incluyen seguro todo riesgo'
    },
    {
      icon: '🕐',
      title: 'Disponible 24/7',
      description: 'Reserva en línea las 24 horas, los 7 días de la semana'
    }
  ];

  testimonials = [
    {
      name: 'María González',
      rating: 5,
      comment: 'Excelente servicio, el vehículo estaba en perfectas condiciones y el proceso fue muy fácil.',
      location: 'Bogotá'
    },
    {
      name: 'Carlos Rodríguez',
      rating: 5,
      comment: 'La mejor opción para alquilar vehículos. Precios justos y atención personalizada.',
      location: 'Medellín'
    },
    {
      name: 'Ana Martínez',
      rating: 4,
      comment: 'Muy recomendado. El proceso de reserva es súper sencillo y los vehículos están impecables.',
      location: 'Cali'
    }
  ];

  constructor() {
    this.setDefaultDates();
  }

  private setDefaultDates(): void {
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    this.searchData.startDate = today.toISOString().split('T')[0];
    this.searchData.endDate = tomorrow.toISOString().split('T')[0];
  }

  onQuickSearch(): void {
    // Navigate to vehicles page with search parameters
    const params = new URLSearchParams();
    if (this.searchData.startDate) params.set('startDate', this.searchData.startDate);
    if (this.searchData.endDate) params.set('endDate', this.searchData.endDate);
    if (this.searchData.vehicleType) params.set('type', this.searchData.vehicleType);
    
    window.location.href = `/vehicles?${params.toString()}`;
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0
    }).format(price);
  }

  generateStars(rating: number): string[] {
    const stars = [];
    for (let i = 1; i <= 5; i++) {
      stars.push(i <= rating ? '★' : '☆');
    }
    return stars;
  }

  scrollToSection(sectionId: string): void {
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
  }
}