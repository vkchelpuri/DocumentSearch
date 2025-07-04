import { Component } from '@angular/core';
import { ApiService } from '../api.service';

interface QAPair {
  question: string;
  answer: string;
}

@Component({
  selector: 'app-qa',
  templateUrl: './qa.component.html',
  styleUrls: ['./qa.component.scss']
})
export class QaComponent {
  question = '';
  chatHistory: QAPair[] = [];
  loading = false;

  constructor(private api: ApiService) {}

  ask(): void {
    if (!this.question.trim()) return;
    this.loading = true;

    this.api.askGlobalQuestion(this.question).subscribe({
      next: (res) => {
        this.chatHistory.push({ question: this.question, answer: res.answer });
        this.question = '';
        this.loading = false;
      },
      error: () => {
        this.chatHistory.push({ question: this.question, answer: '⚠️ Error getting response.' });
        this.loading = false;
      }
    });
  }
}
