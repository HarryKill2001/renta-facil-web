import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="skeleton" [ngClass]="variant" [ngStyle]="customStyles">
      <div class="skeleton-shimmer"></div>
    </div>
  `,
  styleUrls: ['./skeleton.component.css']
})
export class SkeletonComponent {
  @Input() variant: 'text' | 'rect' | 'circle' | 'card' = 'text';
  @Input() width: string = '100%';
  @Input() height: string = '1rem';
  @Input() rounded: boolean = false;

  get customStyles() {
    return {
      width: this.width,
      height: this.height,
      borderRadius: this.rounded ? '50%' : (this.variant === 'card' ? '8px' : '4px')
    };
  }
}