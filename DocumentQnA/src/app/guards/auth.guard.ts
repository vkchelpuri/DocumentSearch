// src/app/guards/auth.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { map, take } from 'rxjs/operators';
import { MatDialog } from '@angular/material/dialog';
import { DialogComponent } from '../dialog/dialog.component';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router,
    private dialog: MatDialog
  ) {}

  /**
   * Determines if a route can be activated.
   * Checks if the user is logged in via AuthService.
   * If not logged in, redirects to the login page.
   * If roles or specific permissions are specified in route data, checks if the user has them.
   * Access is granted if any required role is present OR all required permissions are present.
   * @param route The activated route snapshot.
   * @param state The router state snapshot.
   * @returns An Observable, Promise, or boolean indicating if the route can be activated.
   */
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

    return this.authService.isLoggedIn().pipe(
      take(1),
      map(isLoggedIn => {
        if (!isLoggedIn) {
          return this.router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
        }

        let hasRequiredRole = false;
        let hasAllRequiredPermissions = false;
        let denialReason = 'You do not have permission to access this page.';

        // --- Evaluate Roles ---
        if (route.data['roles']) {
          const requiredRoles = route.data['roles'] as string[];
          hasRequiredRole = requiredRoles.some(role => this.authService.hasRole(role));
        } else {
          hasRequiredRole = true;
        }

        // --- Evaluate Permissions ---
        if (route.data['permissions']) {
          const requiredPermissions = route.data['permissions'] as { claimType: string, expectedValue: string }[];
          hasAllRequiredPermissions = requiredPermissions.every(perm =>
            this.authService.hasPermission(perm.claimType, perm.expectedValue)
          );
        } else {
          hasAllRequiredPermissions = true;
        }

        // --- Final Decision (OR logic) ---
        const accessGranted = hasRequiredRole || hasAllRequiredPermissions;

        if (accessGranted) {
          return true;
        } else {
          this.showAccessDeniedDialog(denialReason);
          return this.router.createUrlTree(['/dashboard']);
        }
      })
    );
  }

  /**
   * Opens a Material dialog to inform the user about access denial.
   * @param message The message to display in the dialog.
   */
  private showAccessDeniedDialog(message: string): void {
    this.dialog.open(DialogComponent, {
      width: '400px',
      disableClose: true,
      data: {
        title: 'Access Denied',
        message: message
      }
    });
  }
}
