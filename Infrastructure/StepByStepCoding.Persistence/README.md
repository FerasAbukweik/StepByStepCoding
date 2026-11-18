# Academic features

Each lecture has at most one code file, enforced by a unique index on `LectureFiles.LectureId`. The lecture navigation is `Lecture.File`. A file is added through the existing create-file endpoint. SignalR subscriptions use a single lecture group.

The nine academic tables are Courses, Sections, Enrollments, Lectures, LectureFiles, Exams, ExamQuestions, ExamAttempts, and Submissions. Users, roles, and refresh tokens remain in Security. Student/instructor/reviewer IDs reference Identity users logically; there are no cross-database foreign keys.

## Pattern

- Domain owns entities, private setters, UTC timestamps, status transitions, and business rules.
- Application uses `Features/<Feature>/Commands/<UseCase>/` and `Queries/<UseCase>/`. Each use case has a request implementing `IRequest`, a FluentValidation validator, and a handler. Query folders include DTOs and mapping extensions.
- Persistence owns one configuration and repository per entity. Repositories inherit the shared `Repository<TEntity, ApplicationDbContext>` directly and implement their Application contracts. All reads are untracked. Repositories never call SaveChanges.
- API controllers inject only IMediator and map API input DTOs to commands. Each controller has a matching `.http` file.

The application uses the default `IUnitOfWork` mapped to `IUnitOfWork<ApplicationDbContext>`. Security continues to inject its own context-specific unit of work. A single commit persists each academic command atomically. Parent exam/lecture versions also change when questions/files change, preventing conflicting publish/end operations from silently overwriting edits.

Academic entities inherit `Auditable`. `ApplicationDbContext.SaveChangesAsync` sets creation fields for added entities and modification fields for modified entities, using UTC and the injected `IUserService`. Auditing is skipped when the service is absent; when no user is signed in, user IDs remain null.

## Database setup

Set `ConnectionStrings__ApplicationConnection`, `ConnectionStrings__SecurityConnection`, and JWT settings through configuration/secrets. DbContexts are registered in each layer's `AddDatabase` extension and resolved by DI. No design-time DbContext factory is used.

```sh
dotnet ef database update --project Infrastructure/StepByStepCoding.Persistence --startup-project API/StepByStepCoding.Api --context ApplicationDbContext
dotnet ef database update --project Infrastructure/StepByStepCoding.Security --startup-project API/StepByStepCoding.Api --context ApplicationSecurityDbContext
```

The migrations are generated; these commands must be run against your configured SQL Server databases. The Identity roles `Admin`, `Instructor`, and `Student` must be created and assigned before using role-protected academic features. Signup currently does not grant these roles automatically.

## API

Academic controllers use `[Authorize]`; their mutation endpoints use `[Authorize(Roles = ...)]` with `nameof(UserRole.Admin)`, `nameof(UserRole.Instructor)`, and `nameof(UserRole.Student)`. Role-only admission is handled at the API boundary. Application still enforces ownership, enrollment, draft visibility, and attempt/question relationships. Lists accept `page` and `recordsPerPage` (maximum 100), plus the parent ID indicated below, and return `X-Total-Count`.

| Route | Parent filter | Commands |
|---|---|---|
| `/api/courses` | — | Create, update, delete (Admin) |
| `/api/sections` | `courseId` | Create, update, delete (Admin) |
| `/api/enrollments` | `sectionId` | Create, activate/deactivate, delete |
| `/api/lectures` | `sectionId` | Create, update, delete while scheduled; start, end |
| `/api/lecturefiles` | `lectureId` | Create, update snapshot, delete before lecture ends |
| `/api/exams` | `sectionId` | Create, update/delete draft; publish, close |
| `/api/examquestions` | `examId` | Create, update, delete while exam is draft |
| `/api/examattempts` | `examId` | Start own attempt, finish own attempt |
| `/api/submissions` | `attemptId` | Submit code, grade/regrade |

Instructors can manage only their assigned sections. Students see enrolled content and only their own attempts/submissions. Draft exams and future questions are hidden from students. Lecture files become visible to enrolled students when the lecture starts.

Editable DTOs include a `version`. Supply the current version from GET when updating, deleting, or changing state. A conflict returns 409; reload before retrying. GET the parent exam/lecture again after editing its questions/files. Deletes use `?version=<guid>` and reject records with dependents instead of cascading away academic history.

## Initial assessment policy

There is one attempt per student/exam and one final submission per attempt/question, enforced by unique indexes. Attempt expiry is the earlier of the exam deadline and the student's start plus the duration. Closed, finished, or expired attempts cannot accept submissions. Questions contain title, description, optional image reference, maximum score, and display order. No answer keys, test cases, answer drafts, or review tables are added.

Submitted code is immutable. Grading stores score, feedback, reviewer ID, and review time on the submission. Only the assigned instructor or Admin can grade, and marks cannot exceed the question's maximum. ImagePath is a reference; file-upload/storage is a separate feature. The unified SignalR hub broadcasts edit deltas from the live Redis code session, deletions, and lecture completion. Ending a lecture freezes edits, commits its final file snapshot together with lecture completion, then clears that Redis version.

Tests use SQLite for full mediator workflows and inspect SQL Server schema generation. They do not connect to a live SQL Server.

Academic commands and queries now use CacheInvalidatingRequest and CacheableRequest. SQL command success clears the shared academic cache root; query keys are caller-scoped and cache hits retain current access checks. Live code and time-sensitive question lists bypass generic caching. See the Application Decorators README.
