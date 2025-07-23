import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:44343/api/Account';
  private tokenSubject: BehaviorSubject<string | null>;
  public token$: Observable<string | null>;

  constructor(private http: HttpClient, private router: Router) {
    this.tokenSubject = new BehaviorSubject<string | null>(localStorage.getItem('jwt_token'));
    this.token$ = this.tokenSubject.asObservable();
  }

  public get currentUserToken(): string | null {
    return this.tokenSubject.value;
  }

  login(credentials: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap(response => {
          if (response && response.token) {
            localStorage.setItem('jwt_token', response.token);
            this.tokenSubject.next(response.token);
          }
        }),
        catchError(error => {
          console.error('Login failed:', error);
          throw error;
        })
      );
  }

  register(userData: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, userData)
      .pipe(
        tap(response => {
          if (response && response.token) {
            localStorage.setItem('jwt_token', response.token);
            this.tokenSubject.next(response.token);
          }
        }),
        catchError(error => {
          console.error('Registration failed:', error);
          throw error;
        })
      );
  }

  logout(): void {
    localStorage.removeItem('jwt_token');
    this.tokenSubject.next(null);
    this.router.navigate(['/login']);
  }

  isLoggedIn(): Observable<boolean> {
    return this.token$.pipe(
      map(token => !!token)
    );
  }

  getDecodedToken(): any | null {
    const token = this.currentUserToken;
    if (token) {
      try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
      } catch (e) {
        console.error('Error decoding token:', e);
        return null;
      }
    }
    return null;
  }

  /**
   * Checks if the logged-in user has a specific custom permission claim.
   * @param claimType The name of the custom claim (e.g., 'CanViewDocuments', 'CanUploadDocuments').
   * @param expectedValue The expected value of the claim (defaults to 'True').
   * @returns True if the user has the claim with the expected value, false otherwise.
   */
  hasPermission(claimType: string, expectedValue: string = 'True'): boolean {
    const decodedToken = this.getDecodedToken();
    if (decodedToken) {
      const claimValue = decodedToken[claimType];

      let result = false;

      if (Array.isArray(claimValue)) {
        result = claimValue.includes(expectedValue);
      } else if (typeof claimValue === 'boolean') {
        result = claimValue === (expectedValue === 'True');
      } else if (typeof claimValue === 'string') {
        result = claimValue.toLowerCase() === expectedValue.toLowerCase();
      }
      return result;
    }
    return false;
  }

  /**
   * Checks if the logged-in user has a specific role.
   * @param roleName The name of the role (e.g., 'Admin', 'User').
   * @returns True if the user has the specified role, false otherwise.
   */
  hasRole(roleName: string): boolean {
    const decodedToken = this.getDecodedToken();
    if (decodedToken) {
      const roles = decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

      let result = false;
      if (Array.isArray(roles)) {
        result = roles.includes(roleName);
      } else if (typeof roles === 'string') {
        result = roles === roleName;
      }
      return result;
    }
    return false;
  }
}
