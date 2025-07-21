import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SkeletonComponent } from '../skeleton/skeleton.component';

@Component({
  selector: 'app-vehicle-card-skeleton',
  standalone: true,
  imports: [CommonModule, SkeletonComponent],
  template: `
    <div class="vehicle-card-skeleton">
      <div class="vehicle-image-skeleton">
        <app-skeleton variant="rect" width="100%" height="180px"></app-skeleton>
      </div>
      
      <div class="vehicle-info-skeleton">
        <app-skeleton variant="text" width="70%" height="1.5rem"></app-skeleton>
        <app-skeleton variant="text" width="50%" height="1rem"></app-skeleton>
        <app-skeleton variant="text" width="40%" height="1rem"></app-skeleton>
        
        <div class="pricing-skeleton">
          <app-skeleton variant="text" width="60%" height="1.25rem"></app-skeleton>
          <app-skeleton variant="text" width="80%" height="1.5rem"></app-skeleton>
        </div>
        
        <div class="status-skeleton">
          <app-skeleton variant="text" width="30%" height="1rem"></app-skeleton>
        </div>
        
        <div class="actions-skeleton">
          <app-skeleton variant="rect" width="100px" height="40px"></app-skeleton>
          <app-skeleton variant="rect" width="100px" height="40px"></app-skeleton>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./vehicle-card-skeleton.component.css']
})
export class VehicleCardSkeletonComponent {}