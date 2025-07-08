import { Component, ViewChild, ElementRef, OnInit, AfterViewChecked } from '@angular/core';
import { ApiService } from '../api.service';

export interface QAPair {
  question: string;
  answer: string;
  sourceDocument?: string;
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
  this.api.loadChat().subscribe({
    next: (res: QAPair[]) => {
      this.chatHistory = res;
      setTimeout(() => this.scrollToBottom(), 0);
    },
    error: err => console.error('Failed to load chat:', err)
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

  /**
   * Saves the current chat history to the server.
   */
  saveChatToServer(): void {
    this.api.saveChat(this.chatHistory).subscribe({
      next: () => console.log('Chat saved.'),
      error: err => console.error('Save failed', err)
    });
  }

  clearLocalChat(): void {
  this.chatHistory = [];
  this.api.clearChat().subscribe({
    next: () => console.log('🗑️ Server chat cleared.'),
    error: err => console.error('❌ Failed to clear server chat:', err)
  });
}
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

  this.chatHistory.push({
    question: userMessage,
    answer: '',
    sourceDocument: ''
  });

  this.loading = true;
  this.question = '';

  // Optional delay for natural feel
  setTimeout(() => {
    this.api.askGlobalQuestion(userMessage).subscribe({
      next: (res: { answer: string; sourceDocument: string }) => {
        const lastIndex = this.chatHistory.length - 1;

        this.chatHistory[lastIndex].answer = res.answer?.trim() || 'No answer received.';
        this.chatHistory[lastIndex].sourceDocument =
          res.sourceDocument?.trim().toLowerCase() !== 'none'
            ? res.sourceDocument.trim()
            : '';

        this.saveChatToServer(); // 🔒 Persist to backend
        this.loading = false;
        this.scrollToBottom();   // 🔽 Scroll to latest reply
      },
      error: (err) => {
        console.error('Gemini API error:', err);

        const lastIndex = this.chatHistory.length - 1;
        this.chatHistory[lastIndex].answer = '⚠️ Error getting response.';
        this.chatHistory[lastIndex].sourceDocument = '';

        this.loading = false;
        this.scrollToBottom(); // 🔽 Even if error
      }
    });
  }, 500);
}

  /**
   * Scrolls the chat thread to the bottom.
   * It finds the last chat row and uses scrollIntoView for smooth scrolling.
   */
private scrollToBottom(): void {
  setTimeout(() => {
    if (this.chatThread?.nativeElement) {
      this.chatThread.nativeElement.scrollTop = this.chatThread.nativeElement.scrollHeight;
    }
  }, 0);
}

  
}