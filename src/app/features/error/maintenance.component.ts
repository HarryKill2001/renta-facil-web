import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-maintenance',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './maintenance.component.html',
  styleUrls: ['./maintenance.component.css']
})
export class MaintenanceComponent {
  estimatedTime = '2 horas';
  currentTime = new Date();

  constructor() {
    // Update time every minute
    setInterval(() => {
      this.currentTime = new Date();
    }, 60000);
  }

  refreshPage(): void {
    window.location.reload();
  }

  get formattedTime(): string {
    return this.currentTime.toLocaleString('es-CO', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}