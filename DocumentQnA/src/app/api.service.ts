import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = 'https://localhost:44343/api';

  constructor(private http: HttpClient) {}

  uploadFile(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.baseUrl}/document/upload`, formData);
  }

  getDocuments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/document/all`);
  }

  askGlobalQuestion(question: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/question/ask`, { userQuestion: question });
  }

  askQuestion(documentId: number, question: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/question/ask`, {
      documentId,
      userQuestion: question
    });
  }
}
