import { Component } from '@angular/core';
import { ApiService } from '../api.service';
import { ViewChild, ElementRef } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { DialogComponent } from '../dialog/dialog.component';

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

    this.api.uploadFile(this.selectedFile).subscribe({
      next: (res) => {
        this.documentId = res.documentId;
        alert(`Uploaded: ${res.fileName}`);
        this.loading = false;
        this.resetFileInput(); // Call this method after successful upload
        const dialogRef = this.dialog.open(DialogComponent,{
          width:'400px',
          disableClose: true
        });
      },
      error: () => {
        alert('⚠️ Upload failed.');
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