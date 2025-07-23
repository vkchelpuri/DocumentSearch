import { Component, ViewChild, ElementRef } from '@angular/core';
import { ApiService } from '../api.service';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { DialogComponent, DialogData } from '../dialog/dialog.component';
import { HttpErrorResponse } from '@angular/common/http';

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
  @ViewChild('fileInput') fileInput!: ElementRef;

  constructor(
    private api: ApiService,
    private dialog: MatDialog,
    private router: Router
  ) { }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
    this.fileName = this.selectedFile?.name || '';
  }

  upload() {
    if (!this.selectedFile || this.loading) return;

    this.loading = true;

    this.api.uploadDocument(this.selectedFile).subscribe({
      next: (res: { documentId: number; fileName: string; message: string }) => {
        this.documentId = res.documentId;
        this.loading = false;
        this.resetFileInput();

        const dialogData: DialogData = {
          title: 'Upload Successful',
          message: `File '${res.fileName}' uploaded successfully!`,
          showUploadButtons: true
        };

        const dialogRef = this.dialog.open(DialogComponent, {
          width: '400px',
          disableClose: true,
          data: dialogData
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result === 'documents') {
            this.router.navigate(['/documents']);
          } else if (result === 'upload') {
            // Stay on upload page, input is already reset
          }
        });
      },
      error: (err: HttpErrorResponse) => {
        console.error('Upload failed:', err);

        const errorMessage = err.error?.Message || 'Upload failed. Please try again.';

        const dialogData: DialogData = {
          title: 'Upload Failed',
          message: errorMessage,
          showUploadButtons: false
        };

        this.dialog.open(DialogComponent, {
          width: '400px',
          disableClose: true,
          data: dialogData
        });
        this.loading = false;
      }
    });
  }

  resetFileInput() {
    this.selectedFile = null;
    this.fileName = '';
    if (this.fileInput && this.fileInput.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
  }
}
