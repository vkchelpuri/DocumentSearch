// src/app/app.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core'; // Import OnInit and OnDestroy
import { AuthService } from './auth/auth.service';
import { Subscription } from 'rxjs'; // Import Subscription

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy { // Implement OnInit and OnDestroy
  title = 'DocumentQnA';
  username: string | null = null; // Property to hold the logged-in username
  private authSubscription!: Subscription; // To manage the subscription

  constructor(public authService: AuthService) { }

  ngOnInit(): void {
    // Subscribe to token changes to update the username
    this.authSubscription = this.authService.token$.subscribe(token => {
      if (token) {
        const decodedToken = this.authService.getDecodedToken();
        // Access the 'name' claim (http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name)
        this.username = decodedToken?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || null;
      } else {
        this.username = null; // Clear username if logged out
      }
    });
  }

  ngOnDestroy(): void {
    // Unsubscribe to prevent memory leaks
    if (this.authSubscription) {
      this.authSubscription.unsubscribe();
    }
  }
}
