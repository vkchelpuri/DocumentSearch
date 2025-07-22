// src/app/login/login.component.ts
import { Component } from '@angular/core';
import { AuthService } from '../auth/auth.service'; // Adjust path if needed
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  username!: string; // Using '!' to assert that it will be initialized
  password!: string; // Using '!' to assert that it will be initialized
  errorMessage: string = '';

  constructor(private authService: AuthService, private router: Router) {}

  /**
   * Handles the login form submission.
   * Calls the AuthService to authenticate the user.
   */
  onLogin(): void {
    // Clear any previous error messages
    this.errorMessage = '';

    // Call the login method from AuthService
    this.authService.login({ username: this.username, password: this.password })
      .subscribe({
        next: () => {
          // On successful login, navigate to the dashboard or a protected route
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          // On error, display the error message from the backend or a generic one
          console.error('Login error:', err);
          this.errorMessage = err.error?.Message || 'Login failed. Please check your credentials.';
        }
      });
  }
}
