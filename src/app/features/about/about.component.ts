import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.css']
})
export class AboutComponent {
  stats = [
    { value: '10+', label: 'Años de experiencia' },
    { value: '150+', label: 'Vehículos disponibles' },
    { value: '5000+', label: 'Clientes satisfechos' },
    { value: '99%', label: 'Satisfacción del cliente' }
  ];

  team = [
    {
      name: 'Carlos Rodríguez',
      position: 'CEO y Fundador',
      description: 'Con más de 15 años en la industria automotriz, Carlos fundó RentaFácil con la visión de hacer el alquiler de vehículos más accesible.',
      image: 'ceo-placeholder'
    },
    {
      name: 'María González',
      position: 'Directora de Operaciones',
      description: 'María supervisa toda la flota y garantiza que cada vehículo esté en perfectas condiciones para nuestros clientes.',
      image: 'operations-placeholder'
    },
    {
      name: 'Luis Martínez',
      position: 'Jefe de Atención al Cliente',
      description: 'Luis y su equipo se aseguran de brindar el mejor servicio y soporte a nuestros clientes las 24 horas.',
      image: 'customer-service-placeholder'
    }
  ];

  values = [
    {
      icon: '🎯',
      title: 'Compromiso',
      description: 'Nos comprometemos a ofrecer vehículos de calidad y un servicio excepcional en cada experiencia.'
    },
    {
      icon: '🔒',
      title: 'Confianza',
      description: 'Construimos relaciones duraderas basadas en la transparencia y la honestidad con nuestros clientes.'
    },
    {
      icon: '🚀',
      title: 'Innovación',
      description: 'Utilizamos la última tecnología para hacer que el proceso de alquiler sea más fácil y eficiente.'
    },
    {
      icon: '🌟',
      title: 'Excelencia',
      description: 'Nos esforzamos por superar las expectativas en cada detalle de nuestro servicio.'
    }
  ];

  milestones = [
    {
      year: '2014',
      title: 'Fundación de RentaFácil',
      description: 'Iniciamos con una pequeña flota de 10 vehículos en Bogotá.'
    },
    {
      year: '2017',
      title: 'Expansión Nacional',
      description: 'Abrimos oficinas en Medellín, Cali y Barranquilla.'
    },
    {
      year: '2020',
      title: 'Plataforma Digital',
      description: 'Lanzamos nuestra plataforma online para reservas 24/7.'
    },
    {
      year: '2023',
      title: 'Flota Premium',
      description: 'Alcanzamos los 150+ vehículos incluyendo modelos premium y eléctricos.'
    }
  ];
}