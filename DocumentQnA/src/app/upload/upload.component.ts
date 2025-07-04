import { Component } from '@angular/core';
import { ApiService } from '../api.service';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss']
})
export class UploadComponent {
  selectedFile: File | null = null;
  documentId: number | null = null;
  fileName = '';

  constructor(private api: ApiService) {}

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
    this.fileName = this.selectedFile?.name || '';
  }

  upload() {
    if (!this.selectedFile) return;

    this.api.uploadFile(this.selectedFile).subscribe(res => {
      this.documentId = res.documentId;
      alert(`Uploaded: ${res.fileName}`);
    });
  }
}
