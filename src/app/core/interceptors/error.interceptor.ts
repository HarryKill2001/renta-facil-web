import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        this.handleError(error);
        return throwError(() => error);
      })
    );
  }

  private handleError(error: HttpErrorResponse): void {
    // Don't redirect for certain errors that should be handled by components
    const skipRedirectStatus = [400, 401, 422]; // Validation errors, auth errors, etc.
    
    if (skipRedirectStatus.includes(error.status)) {
      return;
    }

    // Don't redirect if we're already on an error page
    if (this.router.url.includes('/error') || this.router.url.includes('/404')) {
      return;
    }

    // Map HTTP status codes to error types
    let errorType = '500';
    
    switch (error.status) {
      case 0:
        errorType = 'network';
        break;
      case 403:
        errorType = '403';
        break;
      case 404:
        // Let the router handle 404s
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
        if (error.status >= 500) {
          errorType = '500';
        }
        break;
    }

    // Only redirect for server errors and network issues
    if (['network', 'timeout', '500', '403'].includes(errorType)) {
      this.router.navigate(['/error'], { 
        queryParams: { type: errorType },
        skipLocationChange: false
      });
    }
  }
}