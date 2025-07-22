// src/app/admin-dashboard/admin-dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { ApiService } from '../api.service'; // Assuming ApiService will handle user fetching
import { AuthService } from '../auth/auth.service'; // For permission checks
import { MatTableDataSource } from '@angular/material/table'; // For Material Table
import { HttpErrorResponse } from '@angular/common/http'; // For error handling

// Define an interface for the User data received from the backend
export interface User {
  id: string;
  username: string;
  canViewDocuments: boolean;
  canUploadDocuments: boolean;
  roles: string[];
}

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  displayedColumns: string[] = ['username', 'roles', 'canViewDocuments', 'canUploadDocuments', 'actions'];
  dataSource = new MatTableDataSource<User>();
  loading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(private apiService: ApiService, public authService: AuthService) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  /**
   * Fetches all users from the backend API.
   */
  loadUsers(): void {
    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;

    // Assuming ApiService will have a method to get all users
    // We'll add this method to ApiService next.
    this.apiService.getAllUsers().subscribe({
      next: (users: User[]) => {
        this.dataSource.data = users; // Assign fetched users to the table data source
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        console.error('Failed to load users:', err);
        this.errorMessage = err.error?.Message || 'Failed to load users. You might not have admin privileges.';
        this.loading = false;
      }
    });
  }

  /**
   * Updates a user's permissions (CanViewDocuments, CanUploadDocuments).
   * @param user The user object with potentially updated permission values.
   */
  updatePermissions(user: User): void {
    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;

    // Call the API service to update permissions
    this.apiService.updateUserPermissions(user.id, user.canViewDocuments, user.canUploadDocuments).subscribe({
      next: (res) => {
        this.successMessage = res.Message || 'Permissions updated successfully!';
        this.loading = false;
        // Optionally, reload users to ensure data consistency, or just update local data
        // this.loadUsers();
      },
      error: (err: HttpErrorResponse) => {
        console.error('Failed to update permissions:', err);
        this.errorMessage = err.error?.Message || 'Failed to update permissions.';
        this.loading = false;
      }
    });
  }
}
