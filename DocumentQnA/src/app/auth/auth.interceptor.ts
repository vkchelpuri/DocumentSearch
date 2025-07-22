// src/app/auth/auth.interceptor.ts
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

  constructor(private authService: AuthService, private router: Router) {}

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
        // ONLY log out on 401 Unauthorized (token invalid/expired)
        if (error.status === 401) {
          console.warn('Authentication error (401) detected. Logging out:', error);
          this.authService.logout(); // Clear token and redirect to login
          // The router.navigate to /login is already part of logout()
        } else if (error.status === 403) {
          // For 403 Forbidden, the user is authenticated but not authorized for this specific resource.
          // We do NOT log them out. The AuthGuard or component logic should handle displaying a message.
          console.warn('Authorization error (403) detected. Access denied for resource:', error);
        }
        // Re-throw the error to propagate it to the component that made the request
        return throwError(error);
      })
    );
  }
}
