import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { QAPair } from './qa/qa.component';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = 'https://localhost:44343/api';

  constructor(private http: HttpClient) { }

  uploadFile(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.baseUrl}/document/upload`, formData);
  }

  getDocuments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/document/all`);
  }

  // api.service.ts
  askGlobalQuestion(question: string): Observable<{ answer: string; sourceDocument: string }> {
    return this.http.post<{ answer: string; sourceDocument: string }>(
      `${this.baseUrl}/question/ask`,
      { userQuestion: question }
    );
  }

  askQuestion(documentId: number, question: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/question/ask`, {
      documentId,
      userQuestion: question
    });
  }
  uploadDocument(formData: FormData): Observable<any> {
    return this.http.post(`${this.baseUrl}/document/upload`, formData);
  }

  deleteDocument(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/document/${id}`);
  }
  saveChat(chat: QAPair[]): Observable<any> {
    return this.http.post('/api/chat/save', chat);
  }

  loadChat(): Observable<QAPair[]> {
    return this.http.get<QAPair[]>('/api/chat/load');
  }

  clearChat(): Observable<any> {
    return this.http.delete('/api/chat/clear');
  }


}
