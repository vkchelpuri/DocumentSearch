import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

export interface DialogData {
  title: string;
  message: string;
  showUploadButtons?: boolean;
}

@Component({
  selector: 'app-dialog',
  templateUrl: './dialog.component.html',
  styleUrls: ['./dialog.component.scss']
})
export class DialogComponent {
  constructor(
    public dialogRef: MatDialogRef<DialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData 
  ) {
    if (!this.data) {
      this.data = {
        title: 'Operation Successful',
        message: 'Your request was processed successfully.',
        showUploadButtons: false 
      };
    }
  }


  continueUpload(): void {
    this.dialogRef.close('upload');
  }


  goToDocuments(): void {
    this.dialogRef.close('documents'); 
  }

  closeDialog(): void {
    this.dialogRef.close();
  }
}
