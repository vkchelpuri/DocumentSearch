// src/app/app-routing.module.ts
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Import your new authentication components
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component'; // Corrected typo here
import { DashboardComponent } from './dashboard/dashboard.component';

// Import your AuthGuard
import { AuthGuard } from './guards/auth.guard';

// Import your existing application components
import { DocumentsComponent } from './documents/documents.component';
import { QaComponent } from './qa/qa.component';
import { UploadComponent } from './upload/upload.component';

// NEW IMPORT FOR ADMIN DASHBOARD
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';

const routes: Routes = [
  // Authentication routes (accessible without login)
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  // Protected routes (require login)
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'qa', component: QaComponent, canActivate: [AuthGuard] }, // QA always needs authentication

  // DOCUMENTS route requires CanViewDocuments permission OR Admin role
  {
    path: 'documents',
    component: DocumentsComponent,
    canActivate: [AuthGuard],
    data: {
      permissions: [{ claimType: 'CanViewDocuments', expectedValue: 'True' }],
      roles: ['Admin'] // Admins can also view documents
    }
  },
  // UPLOAD route requires CanUploadDocuments permission OR Admin role
  {
    path: 'upload',
    component: UploadComponent,
    canActivate: [AuthGuard],
    data: {
      permissions: [{ claimType: 'CanUploadDocuments', expectedValue: 'True' }],
      roles: ['Admin'] // Admins can also upload documents
    }
  },

  // Admin Dashboard Route - protected by AuthGuard and requires 'Admin' role
  {
    path: 'admin-dashboard',
    component: AdminDashboardComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] }
  },

  // Default route - redirects to dashboard (which is protected)
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },

  // Wildcard route for any other invalid paths - redirects to dashboard
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
