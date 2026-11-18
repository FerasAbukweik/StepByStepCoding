import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/data/auth-service';

export const staffGuard: CanActivateFn = () =>
  inject(AuthService).canTeach() || inject(Router).createUrlTree(['/access-denied']);
