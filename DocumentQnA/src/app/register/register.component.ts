// src/app/register/register.component.ts
import { Component } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http'; // Import HttpErrorResponse

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  username!: string;
  password!: string;
  errorMessage: string = '';
  successMessage: string = '';

  constructor(private authService: AuthService, private router: Router) {}

  /**
   * Handles the registration form submission.
   * Calls the AuthService to register the new user.
   */
  onRegister(): void {
    // Clear any previous messages
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.register({ username: this.username, password: this.password })
      .subscribe({
        next: () => {
          this.successMessage = 'Registration successful! You can now log in.';
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 2000);
        },
        error: (err: HttpErrorResponse) => { // Explicitly type 'err'
          console.error('Registration error:', err);

          let friendlyErrorMessage = 'Registration failed. Please try again.';

          // Check for specific Identity validation errors
          if (err.status === 400 && err.error && err.error.errors) {
            // Iterate through the errors object to get specific messages
            const errorDetails: string[] = [];
            for (const key in err.error.errors) {
              if (err.error.errors.hasOwnProperty(key)) {
                errorDetails.push(...err.error.errors[key]);
              }
            }
            if (errorDetails.length > 0) {
              friendlyErrorMessage = errorDetails.join('; '); // Join multiple errors
            }
          } else if (err.error && err.error.message) {
            // For general messages returned directly in 'Message' property
            friendlyErrorMessage = err.error.message;
          }

          this.errorMessage = friendlyErrorMessage;
        }
      });
  }
}
