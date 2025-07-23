import { Component, OnInit, OnDestroy } from '@angular/core'; 
import { AuthService } from './auth/auth.service';
import { Subscription } from 'rxjs'; 

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy { 
  title = 'DocumentQnA';
  username: string | null = null; 
  private authSubscription!: Subscription; 

  constructor(public authService: AuthService) { }

  ngOnInit(): void {
    this.authSubscription = this.authService.token$.subscribe(token => {
      if (token) {
        const decodedToken = this.authService.getDecodedToken();
        this.username = decodedToken?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || null;
      } else {
        this.username = null; 
      }
    });
  }

  ngOnDestroy(): void {
    if (this.authSubscription) {
      this.authSubscription.unsubscribe();
    }
  }
}
