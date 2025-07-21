import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { NotificationService } from '../../shared/services/notification.service';

interface ContactForm {
  name: string;
  email: string;
  phone: string;
  subject: string;
  message: string;
  inquiryType: string;
}

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.css']
})
export class ContactComponent {
  contactForm: ContactForm = {
    name: '',
    email: '',
    phone: '',
    subject: '',
    message: '',
    inquiryType: 'general'
  };

  isSubmitting = false;

  inquiryTypes = [
    { value: 'general', label: 'Consulta General' },
    { value: 'reservation', label: 'Reservas' },
    { value: 'support', label: 'Soporte Técnico' },
    { value: 'billing', label: 'Facturación' },
    { value: 'complaint', label: 'Quejas y Reclamos' },
    { value: 'suggestion', label: 'Sugerencias' }
  ];

  contactInfo = [
    {
      icon: '📞',
      title: 'Teléfono',
      details: ['+57 300 123 4567', '+57 1 234 5678'],
      subtitle: 'Línea de atención 24/7'
    },
    {
      icon: '✉️',
      title: 'Email',
      details: ['info&#64;rentafacil.com', 'soporte&#64;rentafacil.com'],
      subtitle: 'Respuesta en menos de 24 horas'
    },
    {
      icon: '📍',
      title: 'Oficina Principal',
      details: ['Carrera 7 # 123-45', 'Bogotá D.C., Colombia'],
      subtitle: 'Lun - Sáb: 8:00 AM - 6:00 PM'
    },
    {
      icon: '💬',
      title: 'WhatsApp',
      details: ['+57 300 123 4567'],
      subtitle: 'Chat directo con nuestro equipo'
    }
  ];

  officeLocations = [
    {
      city: 'Bogotá',
      address: 'Carrera 7 # 123-45',
      phone: '+57 1 234 5678',
      hours: 'Lun - Sáb: 8:00 AM - 6:00 PM'
    },
    {
      city: 'Medellín',
      address: 'Carrera 43A # 65-89',
      phone: '+57 4 567 8901',
      hours: 'Lun - Sáb: 8:00 AM - 6:00 PM'
    },
    {
      city: 'Cali',
      address: 'Avenida 6N # 23-56',
      phone: '+57 2 345 6789',
      hours: 'Lun - Sáb: 8:00 AM - 6:00 PM'
    },
    {
      city: 'Barranquilla',
      address: 'Carrera 53 # 75-12',
      phone: '+57 5 234 5678',
      hours: 'Lun - Sáb: 8:00 AM - 6:00 PM'
    }
  ];

  faqs = [
    {
      question: '¿Cuáles son los requisitos para alquilar un vehículo?',
      answer: 'Necesitas ser mayor de 21 años, tener licencia de conducir vigente (mínimo 2 años), documento de identidad y tarjeta de crédito o débito.'
    },
    {
      question: '¿El seguro está incluido en el precio?',
      answer: 'Sí, todos nuestros vehículos incluyen seguro todo riesgo sin costo adicional.'
    },
    {
      question: '¿Puedo cancelar mi reserva?',
      answer: 'Puedes cancelar tu reserva hasta 24 horas antes sin penalización. Para cancelaciones con menos tiempo, se aplicarán términos específicos.'
    },
    {
      question: '¿Tienen servicio de entrega a domicilio?',
      answer: 'Sí, ofrecemos servicio de entrega y recogida en tu ubicación por un costo adicional según la zona.'
    },
    {
      question: '¿Qué pasa si tengo un problema durante el alquiler?',
      answer: 'Contamos con asistencia 24/7. Puedes llamarnos al +57 300 123 4567 para cualquier emergencia o problema.'
    }
  ];

  expandedFaq: number | null = null;

  constructor(private notificationService: NotificationService) {}

  onSubmit(form: NgForm): void {
    if (!form.valid || this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;

    // Simulate form submission
    setTimeout(() => {
      this.isSubmitting = false;
      this.notificationService.showSuccess(
        'Mensaje enviado',
        'Tu consulta ha sido enviada exitosamente. Te responderemos pronto.'
      );
      
      // Reset form
      this.contactForm = {
        name: '',
        email: '',
        phone: '',
        subject: '',
        message: '',
        inquiryType: 'general'
      };
      form.resetForm();
    }, 2000);
  }

  toggleFaq(index: number): void {
    this.expandedFaq = this.expandedFaq === index ? null : index;
  }

  formatPhoneNumber(): void {
    // Format Colombian phone number
    const cleaned = this.contactForm.phone.replace(/\D/g, '');
    if (cleaned.length === 10) {
      this.contactForm.phone = `+57 ${cleaned.substring(0, 3)} ${cleaned.substring(3, 6)} ${cleaned.substring(6)}`;
    }
  }
}