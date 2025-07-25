import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(private authService: AuthService, private router: Router) { }

  /**
   * Intercepts outgoing HTTP requests to add the JWT token to the Authorization header.
   * Handles 401 errors by logging out the user.
   * @param request The outgoing HTTP request.
   * @param next The next handler in the chain.
   * @returns An Observable of the HTTP event.
   */
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.currentUserToken;

    if (token) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          console.warn('Authentication error (401) detected. Logging out:', error);
          this.authService.logout();
        } else if (error.status === 403) {
          console.warn('Authorization error (403) detected. Access denied for resource:', error);
        }
        return throwError(error);
      })
    );
  }
}
