import { Component, OnInit } from '@angular/core';
import { ApiService } from '../api.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-documents',
  templateUrl: './documents.component.html',
  styleUrls: ['./documents.component.scss']
})
export class DocumentsComponent implements OnInit {
  files: any[] = [];
  columns: string[] = ['fileName', 'uploadedAt', 'actions'];

  constructor(private api: ApiService, private router: Router) {}

  ngOnInit(): void {
    this.loadFiles();
  }

  loadFiles() {
    this.api.getAllDocuments().subscribe(res => this.files = res);
  }

  delete(id: number) {
    this.api.deleteDocument(id).subscribe(() => this.loadFiles());
  }

  edit(file: any) {
    alert(`Editing: ${file.fileName}`); // Add modal/edit later
  }

  goToUploadPage() {
    this.router.navigate(['/upload']);
  }
}
