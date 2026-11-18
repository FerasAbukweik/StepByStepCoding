import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-account-help',
  imports: [RouterLink],
  template: `<main class="min-h-dvh bg-app flex items-center justify-center px-4 py-10 sm:px-6">
    <div class="w-full max-w-md bg-surface border border-borderPrimary rounded-lg p-6 sm:p-8">
      <p class="text-xs font-mono text-textMuted mb-6">CodeClass / Account support</p>
      <h1 class="text-2xl font-semibold">Need help signing in?</h1>
      <p class="mt-4 text-sm text-textSecondary">
        University accounts are managed by your department administrator. Contact your department to
        reset your password or restore access.
      </p>
      <a routerLink="/login" class="inline-block text-accentBlue mt-6 text-sm"
        >← Return to sign in</a
      >
    </div>
  </main>`,
})
export class AccountHelpComponent {}
