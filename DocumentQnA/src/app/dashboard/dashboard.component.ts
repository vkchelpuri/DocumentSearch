// src/app/dashboard/dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { AuthService } from '../auth/auth.service'; // Adjust path if needed

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  username: string | null = null;

  constructor(public authService: AuthService) { } // Make authService public for template access

  ngOnInit(): void {
    // Get the username from the decoded token when the dashboard loads
    const decodedToken = this.authService.getDecodedToken();
    if (decodedToken && decodedToken.hasOwnProperty('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')) {
      this.username = decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
    }
  }
}
