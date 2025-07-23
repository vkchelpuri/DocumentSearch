import { Component, ViewChild, ElementRef, OnInit, AfterViewChecked } from '@angular/core';
import { ApiService } from '../api.service';
import { HttpErrorResponse } from '@angular/common/http';

export interface QAPair {
  question: string;
  answer: string;
  sourceDocument?: string;
  timestamp?: Date;
  userId?: string;
  loading?: boolean; // 🆕 New flag for bubble animation
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

  private shouldScroll = false;

  constructor(private api: ApiService) { }

  ngOnInit(): void {
    this.api.getChatHistory().subscribe({
      next: (res: QAPair[]) => {
        this.chatHistory = res;
        setTimeout(() => this.scrollToBottom(), 0);
      },
      error: (err: HttpErrorResponse) => {
        console.error('Failed to load chat:', err);
      }
    });
  }

  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  clearLocalChat(): void {
    this.chatHistory = [];
    this.api.clearChatHistory().subscribe({
      next: () => console.log('🗑️ Server chat cleared.'),
      error: (err: HttpErrorResponse) => {
        console.error('❌ Failed to clear server chat:', err);
      }
    });
  }

  clearChatHandler(): void {
    if (confirm('This will delete all saved chats. Are you sure?')) {
      this.clearLocalChat();
    }
  }

  ask(): void {
    if (!this.question.trim() || this.loading) return;

    const userMessage = this.question.trim();
    this.loading = true;
    this.shouldScroll = true;
    this.question = '';

    // 🆕 Push a bubble with loading=true
    this.chatHistory.push({
      question: userMessage,
      answer: '',
      sourceDocument: '',
      loading: true
    });

    setTimeout(() => {
      this.api.askChatQuestion(userMessage).subscribe({
        next: (res: { response: string; sourceDocument: string }) => {
          const lastIndex = this.chatHistory.length - 1;

          this.chatHistory[lastIndex].answer = res.response?.trim() || 'No answer received.';
          this.chatHistory[lastIndex].sourceDocument =
            res.sourceDocument?.trim().toLowerCase() !== 'none'
              ? res.sourceDocument.trim()
              : '';

          this.chatHistory[lastIndex].loading = false; // 🔁 Stop animation
          this.loading = false;
          this.shouldScroll = true;
        },
        error: (err: HttpErrorResponse) => {
          console.error('Gemini API error:', err);

          const lastIndex = this.chatHistory.length - 1;
          this.chatHistory[lastIndex].answer = '⚠️ Error getting response.';
          this.chatHistory[lastIndex].sourceDocument = '';
          this.chatHistory[lastIndex].loading = false;

          this.loading = false;
          this.shouldScroll = true;
        }
      });
    }, 500);
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      if (this.chatThread?.nativeElement) {
        this.chatThread.nativeElement.scrollTop = this.chatThread.nativeElement.scrollHeight;
      }
    }, 0);
  }
}
