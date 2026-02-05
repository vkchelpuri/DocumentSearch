# Document Search AI - Project Documentation

## PROJECT OVERVIEW

Document Search AI is my **personal project**, and I delivered it **end-to-end as the sole developer**. The platform enables users to upload documents, extract and index text content, and ask natural-language questions with AI-generated answers grounded in uploaded files. I built the complete solution using ASP.NET Core Web API, Angular, SQL Server, JWT authentication, and Gemini APIs (generation + embeddings).

## SOLO OWNERSHIP & CONTRIBUTIONS

I independently handled the full software lifecycle for this project:

- Product thinking and feature planning
- Backend architecture and API development
- Database design, EF Core migrations, and persistence
- AI integration for embeddings and answer generation
- Frontend implementation and UX flows
- Security implementation (authentication + authorization)
- Testing, debugging, and iterative improvements

## WHAT I BUILT

### 1) Document Upload, Extraction & Indexing

- Built upload APIs to validate files, process content, and save metadata.
- Implemented text extraction support for **PDF, DOCX, TXT, CSV, and XLSX**.
- Added embedding generation and storage so uploaded content can be semantically searched.
- Added temporary-file cleanup and exception handling in the ingestion flow.

### 2) AI Question Answering with Semantic Retrieval

- Built Q&A APIs to answer user questions from document context.
- Implemented semantic retrieval with cosine similarity over embeddings.
- Added relevance controls (similarity threshold + top-N document selection).
- Returned answers with source-document context for traceability.

### 3) Chat Workflow & History Management

- Built authenticated chat endpoints for asking questions and receiving responses.
- Persisted chat history (question, answer, source, timestamp) per user.
- Implemented admin-only chat clearing for operational maintenance.

### 4) Authentication, RBAC & Permission Controls

- Implemented ASP.NET Core Identity with JWT token-based authentication.
- Added role-based and claim-based authorization for document access.
- Built login/register APIs and initial admin-seeding behavior.
- Built admin APIs for updating user document permissions.

### 5) Angular Frontend Modules

- Built frontend modules for login, registration, dashboard, upload, documents, Q&A chat, and admin dashboard.
- Protected routes using `AuthGuard` with permission/role metadata.
- Implemented API service integration for uploads, Q&A, history, documents, and admin actions.

### 6) API & Platform Engineering

- Configured CORS, dependency injection, and Swagger/OpenAPI with JWT bearer auth.
- Integrated Gemini generation and embedding APIs with structured error handling and rate-limit handling.
- Maintained a clean controller/service abstraction model (`IAuthService`, `IGeminiServices`, `ITextExtractor`).

## TECHNICAL SKILLS DEMONSTRATED

- **Backend:** ASP.NET Core Web API, ASP.NET Core Identity, Entity Framework Core, JWT, Swagger/OpenAPI
- **Frontend:** Angular, TypeScript, route guards, service-based API integration, Angular Material
- **AI/RAG:** Gemini Generate API, Gemini Embedding API, semantic retrieval, cosine similarity
- **Database:** SQL Server, EF Core migrations, document and chat persistence
- **Security:** Role-based authorization, claim-based access control
- **Document Processing:** Multi-format extraction pipeline (PDF/Word/Excel/CSV/Text)

## BUSINESS IMPACT

- Reduced manual effort in finding answers from long documents.
- Improved response trust by tying answers to source documents.
- Enabled controlled access to document operations through admin-managed permissions.
- Delivered one unified workflow for upload, search, and conversational Q&A.

## CHALLENGES I SOLVED

- Unified extraction logic for multiple document formats.
- Reduced irrelevant AI output via retrieval grounding and strict prompting strategy.
- Balanced usability and enterprise security with claims + roles.
- Handled external AI dependency issues with robust error/rate-limit handling.

## SUMMARY

I independently built Document Search AI from concept to implementation as a **solo full-stack project**, covering ingestion, indexing, semantic retrieval, AI Q&A, chat history, admin governance, and secure access control using **ASP.NET Core, Angular, EF Core, SQL Server, and Gemini APIs**.
