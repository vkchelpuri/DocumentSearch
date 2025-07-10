import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-dialog',
  templateUrl: './dialog.component.html',
  styleUrls: ['./dialog.component.scss']
})
export class DialogComponent {
  constructor(public dialogRef: MatDialogRef<DialogComponent>) {}

  continueUpload(): void {
    this.dialogRef.close('upload');
  }

goToDocuments(): void {
  this.dialogRef.close(); // Close the dialog first
  window.location.href = '/documents'; // Navigate to documents route
}

}
