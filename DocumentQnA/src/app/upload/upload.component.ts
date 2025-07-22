import { Component, ViewChild, ElementRef, OnInit, AfterViewChecked } from '@angular/core';
import { ApiService } from '../api.service';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { DialogComponent } from '../dialog/dialog.component';
import { HttpErrorResponse } from '@angular/common/http'; // Import HttpErrorResponse for error typing

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss']
})
export class UploadComponent {
  selectedFile: File | null = null;
  documentId: number | null = null;
  fileName = '';
  loading = false;
  @ViewChild('fileInput') fileInput!: ElementRef; // Ensure this matches the template

  constructor(
    private api: ApiService,
    private dialog: MatDialog,
    private router: Router
  ) {}

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
    this.fileName = this.selectedFile?.name || '';
  }

  upload() {
    if (!this.selectedFile || this.loading) return;

    this.loading = true;

    // UPDATED: Changed uploadFile() to uploadDocument() and explicitly typed 'res'
    this.api.uploadDocument(this.selectedFile).subscribe({
      next: (res: { documentId: number; fileName: string; message: string }) => { // Explicitly type 'res'
        this.documentId = res.documentId;
        // Replaced alert with console.log as alerts are discouraged in Angular Material contexts
        console.log(`Uploaded: ${res.fileName}. Message: ${res.message}`);
        this.loading = false;
        this.resetFileInput(); // Call this method after successful upload

        // Open dialog to confirm upload
        const dialogRef = this.dialog.open(DialogComponent, {
          width: '400px',
          disableClose: true,
          data: { title: 'Upload Successful', message: `File '${res.fileName}' uploaded successfully!` } // Pass data to dialog
        });

        // Optionally, subscribe to dialog close if you need to do something after it's closed
        dialogRef.afterClosed().subscribe(result => {
          console.log('The dialog was closed');
          // Example: navigate to documents page after upload
          this.router.navigate(['/documents']);
        });
      },
      error: (err: HttpErrorResponse) => { // Explicitly type 'err'
        console.error('Upload failed:', err);
        // Display a more informative error message
        const errorMessage = err.error?.Message || 'Upload failed. Please try again.';
        this.dialog.open(DialogComponent, {
          width: '400px',
          disableClose: true,
          data: { title: 'Upload Failed', message: errorMessage }
        });
        this.loading = false;
        // Optionally, you might want to reset on error too, or handle differently
        // this.resetFileInput();
      }
    });
  }

  // New method to reset the file input
  resetFileInput() {
    this.selectedFile = null; // Clear the stored file
    this.fileName = ''; // Clear the file name display

    // This is the crucial part: reset the native input element
    if (this.fileInput && this.fileInput.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
  }
}
