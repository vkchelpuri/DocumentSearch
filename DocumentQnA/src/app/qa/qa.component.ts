import { Component, ViewChild, ElementRef, OnInit, AfterViewChecked } from '@angular/core';
import { ApiService } from '../api.service';
import { HttpErrorResponse } from '@angular/common/http'; // Import HttpErrorResponse

export interface QAPair {
  question: string;
  answer: string;
  sourceDocument?: string; // Make sourceDocument optional as it might not always be present
  timestamp?: Date; // Add timestamp if you want to display it
  userId?: string; // Add userId if you want to display it or filter
}

@Component({
  selector: 'app-qa',
  templateUrl: './qa.component.html',
  styleUrls: ['./qa.component.scss']
})
export class QaComponent implements OnInit, AfterViewChecked {
  question = '';
  chatHistory: QAPair[] = [];
  loading = false;
  @ViewChild('chatThread') chatThread!: ElementRef;

  // Flag to control scrolling after DOM updates
  private shouldScroll = false;

  constructor(private api: ApiService) { }

  ngOnInit(): void {
    // UPDATED: Changed loadChat() to getChatHistory()
    this.api.getChatHistory().subscribe({
      next: (res: QAPair[]) => { // Explicitly type 'res'
        this.chatHistory = res;
        // Ensure scrolling happens after the DOM is updated
        setTimeout(() => this.scrollToBottom(), 0);
      },
      error: (err: HttpErrorResponse) => { // Explicitly type 'err'
        console.error('Failed to load chat:', err);
        // Optionally display an error message to the user
      }
    });
  }

  /**
   * Lifecycle hook called after every check of the component's view and its child views.
   * Used here to ensure scrolling happens after the DOM has been updated with new messages.
   */
  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false; // Reset the flag to prevent continuous scrolling
    }
  }

  // REMOVED: saveChatToServer() method as chat saving is now handled by the backend's ask endpoint.
  // The backend's ChatController.AskQuestion endpoint now saves the Q&A pair automatically.

  /**
   * Clears the local chat history and calls the backend to clear server-side chat.
   */
  clearLocalChat(): void {
    this.chatHistory = []; // Clear UI immediately for responsiveness
    // UPDATED: Changed clearChat() to clearChatHistory()
    this.api.clearChatHistory().subscribe({
      next: () => console.log('🗑️ Server chat cleared.'),
      error: (err: HttpErrorResponse) => { // Explicitly type 'err'
        console.error('❌ Failed to clear server chat:', err);
        // Optionally re-fetch chat history or display an error if clearing failed
      }
    });
  }

  /**
   * Handler for the clear chat button, includes a confirmation dialog.
   */
  clearChatHandler(): void {
    if (confirm('This will delete all saved chats. Are you sure?')) {
      this.clearLocalChat(); // clears UI and calls backend
    }
  }

  /**
   * Handles asking a new question to the AI.
   */
  ask(): void {
    if (!this.question.trim() || this.loading) return;

    const userMessage = this.question.trim();

    // Add user's question to chat history immediately
    this.chatHistory.push({
      question: userMessage,
      answer: '', // Placeholder for AI's answer
      sourceDocument: '' // Placeholder for source document
    });

    this.loading = true; // Set loading state
    this.question = ''; // Clear input field

    // Set shouldScroll flag to true to trigger scrolling after DOM update
    this.shouldScroll = true;

    // Optional delay for natural feel (e.g., 500ms)
    setTimeout(() => {
      // UPDATED: Changed askGlobalQuestion() to askChatQuestion()
      this.api.askChatQuestion(userMessage).subscribe({
        next: (res: { response: string; sourceDocument: string }) => { // Explicitly type 'res'
          const lastIndex = this.chatHistory.length - 1;

          // Update the last entry with the AI's answer and source document
          this.chatHistory[lastIndex].answer = res.response?.trim() || 'No answer received.';
          this.chatHistory[lastIndex].sourceDocument =
            res.sourceDocument?.trim().toLowerCase() !== 'none'
              ? res.sourceDocument.trim()
              : '';

          // REMOVED: saveChatToServer() call here, as saving is handled by backend's ask endpoint.

          this.loading = false; // End loading state
          this.shouldScroll = true; // Ensure scroll after update
        },
        error: (err: HttpErrorResponse) => { // Explicitly type 'err'
          console.error('Gemini API error:', err);

          const lastIndex = this.chatHistory.length - 1;
          this.chatHistory[lastIndex].answer = '⚠️ Error getting response.';
          this.chatHistory[lastIndex].sourceDocument = ''; // Clear source on error

          this.loading = false; // End loading state
          this.shouldScroll = true; // Ensure scroll even on error
          // Optionally display an error message to the user in the UI
        }
      });
    }, 500); // Small delay for UX
  }

  /**
   * Scrolls the chat thread to the bottom.
   * It finds the last chat row and uses scrollIntoView for smooth scrolling.
   */
  private scrollToBottom(): void {
    // Use setTimeout with 0ms to ensure the DOM has rendered the new messages
    setTimeout(() => {
      if (this.chatThread?.nativeElement) {
        this.chatThread.nativeElement.scrollTop = this.chatThread.nativeElement.scrollHeight;
      }
    }, 0);
  }
}
