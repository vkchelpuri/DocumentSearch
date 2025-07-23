import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { DashboardComponent } from './dashboard/dashboard.component';

import { AuthGuard } from './guards/auth.guard';

import { DocumentsComponent } from './documents/documents.component';
import { QaComponent } from './qa/qa.component';
import { UploadComponent } from './upload/upload.component';

import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'qa', component: QaComponent, canActivate: [AuthGuard] },
  {
    path: 'documents',
    component: DocumentsComponent,
    canActivate: [AuthGuard],
    data: {
      permissions: [{ claimType: 'CanViewDocuments', expectedValue: 'True' }],
      roles: ['Admin']
    }
  },
  {
    path: 'upload',
    component: UploadComponent,
    canActivate: [AuthGuard],
    data: {
      permissions: [{ claimType: 'CanUploadDocuments', expectedValue: 'True' }],
      roles: ['Admin']
    }
  },

  {
    path: 'admin-dashboard',
    component: AdminDashboardComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] }
  },

  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },

  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
