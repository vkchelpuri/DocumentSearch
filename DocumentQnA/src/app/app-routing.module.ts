import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UploadComponent } from './upload/upload.component';
import { QaComponent } from './qa/qa.component';

const routes: Routes = [
  { path: 'upload', component: UploadComponent },
  { path: 'qa', component: QaComponent },
  { path: '', redirectTo: 'upload', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
