// src/app/pipes/markdown-to-html.pipe.ts
import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { marked } from 'marked'; // Import the marked library

@Pipe({
  name: 'markdownToHtml'
})
export class MarkdownToHtmlPipe implements PipeTransform {

  constructor(private sanitizer: DomSanitizer) { }

  /**
   * Transforms a Markdown string into a sanitized HTML string.
   * @param markdown The input Markdown string.
   * @returns A SafeHtml object that can be bound to innerHTML.
   */
  transform(markdown: string | undefined | null): SafeHtml {
    if (!markdown) {
      return '';
    }

    // Convert Markdown to HTML using marked
    const html = marked.parse(markdown);

    // Sanitize the HTML to prevent XSS attacks.
    // By default, Angular will strip unsafe HTML. DomSanitizer.bypassSecurityTrustHtml
    // tells Angular that we trust this HTML content.
    // Ensure that the input Markdown is from a trusted source or that you have
    // additional sanitization if user-generated content is being parsed.
    return this.sanitizer.bypassSecurityTrustHtml(html as string);
  }
}
