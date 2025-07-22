import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { QAPair } from './qa/qa.component'; // Assuming QAPair is defined here for chat history
import { User } from './admin-dashboard/admin-dashboard.component'; // Import User interface

@Injectable({ providedIn: 'root' })
export class ApiService {
  // IMPORTANT: Ensure this matches your backend API's base URL
  private baseUrl = 'https://localhost:44343/api';

  constructor(private http: HttpClient) { }

  /**
   * Uploads a document file to the backend.
   * This corresponds to the POST /api/Document/upload endpoint.
   * The AuthInterceptor will add the JWT token.
   * @param file The file to upload.
   * @returns An Observable of the API response.
   */
  uploadDocument(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file); // 'file' must match the parameter name in your backend controller
    return this.http.post(`${this.baseUrl}/Document/upload`, formData);
  }

  /**
   * Retrieves all documents from the backend.
   * This corresponds to the GET /api/Document/all endpoint.
   * The AuthInterceptor will add the JWT token.
   * @returns An Observable of an array of documents.
   */
  getAllDocuments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Document/all`);
  }

  /**
   * Deletes a document by its ID.
   * This corresponds to the DELETE /api/Document/{id} endpoint.
   * This endpoint is typically restricted to Admin users on the backend.
   * The AuthInterceptor will add the JWT token.
   * @param id The ID of the document to delete.
   * @returns An Observable of the API response.
   */
  deleteDocument(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Document/${id}`);
  }

  /**
   * Sends a question to the backend's chat endpoint for AI processing.
   * This corresponds to the POST /api/Chat/ask endpoint.
   * The backend will save the question and AI's answer to the user's chat history.
   * The AuthInterceptor will add the JWT token.
   * @param question The user's question string.
   * @returns An Observable of the API response, which includes the AI's answer and source document.
   */
  askChatQuestion(question: string): Observable<{ response: string; sourceDocument: string }> {
    // The backend's ChatController.AskQuestion expects a DTO with 'query' property
    return this.http.post<{ response: string; sourceDocument: string }>(
      `${this.baseUrl}/Chat/ask`,
      { query: question }
    );
  }

  /**
   * Loads the chat history for the current user from the backend.
   * This corresponds to the GET /api/Chat/history endpoint.
   * The AuthInterceptor will add the JWT token.
   * @returns An Observable of an array of QAPair (chat history entries).
   */
  getChatHistory(): Observable<QAPair[]> {
    return this.http.get<QAPair[]>(`${this.baseUrl}/Chat/history`);
  }

  /**
   * Clears the entire chat history.
   * This corresponds to the DELETE /api/Chat/clear endpoint.
   * This endpoint is typically restricted to Admin users on the backend.
   * The AuthInterceptor will add the JWT token.
   * @returns An Observable of the API response.
   */
  clearChatHistory(): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Chat/clear`);
  }

  // NEW: Admin Dashboard API Methods

  /**
   * Retrieves a list of all users from the backend.
   * This corresponds to the GET /api/Account/users endpoint.
   * Requires Admin role.
   * @returns An Observable of an array of User objects.
   */
  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.baseUrl}/Account/users`);
  }

  /**
   * Updates the permissions for a specific user.
   * This corresponds to the PUT /api/Account/permissions/{userId} endpoint.
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
