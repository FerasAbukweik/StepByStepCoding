import { staffGuard } from './core/guards/staff.guard';
import { unsavedChangesGuard } from './core/guards/unsaved-changes.guard';
import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'account-help',
    loadComponent: () =>
      import('./features/auth/account-help.component').then((m) => m.AccountHelpComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./layout/main-layout.component').then((m) => m.MainLayoutComponent),
    children: [
      {
        path: 'exams',
        loadComponent: () =>
          import('./features/exams/exams.component').then((m) => m.ExamsComponent),
      },
      {
        path: 'exams/:examId',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/exams/detail/exam-detail.component').then(
            (m) => m.ExamDetailComponent,
          ),
      },
      {
        path: 'users',
        canActivate: [staffGuard],
        loadComponent: () =>
          import('./features/users/users.component').then((m) => m.UsersComponent),
      },
      {
        path: 'account',
        loadComponent: () =>
          import('./features/account/account.component').then((m) => m.AccountComponent),
      },
      {
        path: 'access-denied',
        data: { code: '403' },
        loadComponent: () =>
          import('./features/fallback/fallback.component').then((m) => m.FallbackComponent),
      },
      {
        path: 'unavailable',
        data: { code: '503' },
        loadComponent: () =>
          import('./features/fallback/fallback.component').then((m) => m.FallbackComponent),
      },
      {
        path: 'not-found',
        data: { code: '404' },
        loadComponent: () =>
          import('./features/fallback/fallback.component').then((m) => m.FallbackComponent),
      },
      {
        path: 'workspace',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/workspace/workspace.component').then((m) => m.WorkspaceComponent),
      },
      {
        path: '',
        loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent),
      },
      {
        path: 'courses',
        loadComponent: () =>
          import('./features/courses/courses.component').then((m) => m.CoursesComponent),
      },
      {
        path: 'courses/:sectionId',
        loadComponent: () =>
          import('./features/courses/section/section.component').then((m) => m.SectionComponent),
      },
      {
        path: 'lectures/:lectureId',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/classroom/classroom.component').then((m) => m.ClassroomComponent),
      },
    ],
  },
  { path: '**', redirectTo: 'not-found' },
];
