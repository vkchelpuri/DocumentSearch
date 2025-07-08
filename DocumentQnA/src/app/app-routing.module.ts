import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UploadComponent } from './upload/upload.component';
import { QaComponent } from './qa/qa.component';
import { DocumentsComponent } from './documents/documents.component';

const routes: Routes = [
  { path: 'upload', component: UploadComponent },
  { path: 'qa', component: QaComponent },
  { path: 'documents', component: DocumentsComponent },
  { path: '', redirectTo: '/documents', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
