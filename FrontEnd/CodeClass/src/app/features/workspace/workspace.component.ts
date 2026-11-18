import { ChangeDetectionStrategy, Component, inject, viewChild } from '@angular/core';
import { AuthService } from '../../core/services/data/auth-service';
import { CodePageComponent } from '../../shared/components/code-page/code-page.component';

@Component({
  selector: 'app-workspace',
  imports: [CodePageComponent],
  template: `<div class="flex min-h-full flex-col">
    <header class="bg-surface p-4 md:p-6">
      <p class="mb-2 text-xs font-mono text-textMuted">CodeClass / My workspace</p>
      <h1 class="text-xl font-semibold">My workspace</h1>
      <p class="mt-2 text-sm text-textSecondary">
        Your personal code file. Changes are saved to your account and synchronized across your open
        sessions.
      </p>
    </header>
    @if (auth.user()?.fileId; as id) {
      <app-code-page [fileId]="id" />
    }
  </div>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkspaceComponent {
  private readonly editor = viewChild(CodePageComponent);

  protected readonly auth = inject(AuthService);

  hasUnsavedChanges(): boolean {
    return this.editor()?.hasUnsavedChanges() ?? false;
  }
}
