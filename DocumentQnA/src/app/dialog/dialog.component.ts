// src/app/dialog/dialog.component.ts
import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

// Define an interface for the data the dialog expects
export interface DialogData {
  title: string;
  message: string;
  // Optional properties for buttons, if needed to make the dialog even more generic
  showUploadButtons?: boolean;
}

@Component({
  selector: 'app-dialog',
  templateUrl: './dialog.component.html',
  styleUrls: ['./dialog.component.scss']
})
export class DialogComponent {
  // Inject MAT_DIALOG_DATA to receive the data object
  constructor(
    public dialogRef: MatDialogRef<DialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData // Inject the data
  ) {
    // Set default values if data isn't provided (e.g., for existing usages)
    if (!this.data) {
      this.data = {
        title: 'Operation Successful',
        message: 'Your request was processed successfully.',
        showUploadButtons: false // Default to not showing upload buttons
      };
    }
  }

  /**
   * Closes the dialog and indicates the user wants to continue uploading.
   * This is specific to the upload success dialog.
   */
  continueUpload(): void {
    this.dialogRef.close('upload');
  }

  /**
   * Closes the dialog and indicates the user wants to go to documents.
   * This is specific to the upload success dialog.
   */
  goToDocuments(): void {
    this.dialogRef.close('documents'); // Pass a specific value to indicate navigation
  }

  /**
   * Closes the dialog without any specific action.
   * This is a generic close for simple informational dialogs.
   */
  closeDialog(): void {
    this.dialogRef.close();
  }
}
