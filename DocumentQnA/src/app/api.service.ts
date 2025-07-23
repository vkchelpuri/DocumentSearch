import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { QAPair } from './qa/qa.component';
import { User } from './admin-dashboard/admin-dashboard.component';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = 'https://localhost:44343/api';

  constructor(private http: HttpClient) { }

  /**
   * Uploads a document file to the backend.
   * @param file The file to upload.
   * @returns An Observable of the API response.
   */
  uploadDocument(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.baseUrl}/Document/upload`, formData);
  }

  /**
   * Retrieves all documents from the backend.
   * @returns An Observable of an array of documents.
   */
  getAllDocuments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Document/all`);
  }

  /**
   * Deletes a document by its ID.
   * @param id The ID of the document to delete.
   * @returns An Observable of the API response.
   */
  deleteDocument(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Document/${id}`);
  }

  /**
   * Sends a question to the backend's chat endpoint for AI processing.
   * @param question The user's question string.
   * @returns An Observable of the API response, which includes the AI's answer and source document.
   */
  askChatQuestion(question: string): Observable<{ response: string; sourceDocument: string }> {
    return this.http.post<{ response: string; sourceDocument: string }>(
      `${this.baseUrl}/Chat/ask`,
      { query: question }
    );
  }

  /**
   * Loads the chat history for the current user from the backend.
   * @returns An Observable of an array of QAPair (chat history entries).
   */
  getChatHistory(): Observable<QAPair[]> {
    return this.http.get<QAPair[]>(`${this.baseUrl}/Chat/history`);
  }

  /**
   * Clears the entire chat history.
   * @returns An Observable of the API response.
   */
  clearChatHistory(): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Chat/clear`);
  }

  /**
   * Retrieves a list of all users from the backend.
   * Requires Admin role.
   * @returns An Observable of an array of User objects.
   */
  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.baseUrl}/Account/users`);
  }

  /**
   * Updates the permissions for a specific user.
   * Requires Admin role.
   * @param userId The ID of the user to update.
   * @param canViewDocuments New value for CanViewDocuments.
   * @param canUploadDocuments New value for CanUploadDocuments.
   * @returns An Observable of the API response.
   */
  updateUserPermissions(userId: string, canViewDocuments: boolean, canUploadDocuments: boolean): Observable<any> {
    return this.http.put(`${this.baseUrl}/Account/permissions/${userId}`, { canViewDocuments, canUploadDocuments });
  }
}
