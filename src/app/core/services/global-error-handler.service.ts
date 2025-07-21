import { Injectable, ErrorHandler } from '@angular/core';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class GlobalErrorHandlerService implements ErrorHandler {
  constructor(private router: Router) {}

  handleError(error: any): void {
    console.error('Global error caught:', error);

    // Handle HTTP errors
    if (error instanceof HttpErrorResponse) {
      this.handleHttpError(error);
      return;
    }

    // Handle other types of errors
    if (error.message?.includes('ChunkLoadError') || error.message?.includes('Loading chunk')) {
      this.handleChunkLoadError();
      return;
    }

    // For development, log the error
    if (!this.isProduction()) {
      console.error('Unhandled error:', error);
    }

    // In production, redirect to a generic error page
    if (this.isProduction()) {
      this.navigateToErrorPage('500');
    }
  }

  private handleHttpError(error: HttpErrorResponse): void {
    let errorType = '500';

    switch (error.status) {
      case 0:
        errorType = 'network';
        break;
      case 400:
        errorType = '400';
        break;
      case 401:
        errorType = '401';
        break;
      case 403:
        errorType = '403';
        break;
      case 404:
        // Don't redirect 404s, let the router handle them
        return;
      case 408:
        errorType = 'timeout';
        break;
      case 500:
      case 502:
      case 503:
      case 504:
        errorType = '500';
        break;
      default:
        errorType = '500';
    }

    this.navigateToErrorPage(errorType);
  }

  private handleChunkLoadError(): void {
    // Chunk load errors usually happen when the app is updated
    // and the user has an old version cached
    if (confirm('Hay una nueva versión disponible. ¿Deseas recargar la página?')) {
      window.location.reload();
    }
  }

  private navigateToErrorPage(errorType: string): void {
    // Avoid infinite loops by checking current route
    if (!this.router.url.includes('/error')) {
      this.router.navigate(['/error'], { 
        queryParams: { type: errorType },
        skipLocationChange: false
      });
    }
  }

  private isProduction(): boolean {
    // In a real app, you'd check environment variables
    return false; // Set to true for production builds
  }

  // Static method for manually handling errors
  static handleApiError(error: HttpErrorResponse, router: Router): void {
    const handler = new GlobalErrorHandlerService(router);
    handler.handleHttpError(error);
  }
}