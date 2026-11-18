import { Injectable, inject } from '@angular/core';
import { tap } from 'rxjs';
import { UsersApiService } from '../api/users-api-service';
import { ResourceState } from '../../Utilities/resource-state';
import { SignupDTO } from '../../DTO/auth/signup.dto';
import { UserLookupDTO } from '../../DTO/auth/user-lookup.dto';

@Injectable({ providedIn: 'root' })
export class UsersService extends ResourceState<UserLookupDTO, UserLookupDTO> {
  private readonly api = inject(UsersApiService);

  search(role: 'Instructor' | 'Student', term = '') {
    return this.read(this.api.search(role, term)).pipe(
      tap((items) => {
        this.items.set(new Map([[1, items]]));
      }),
    );
  }

  create(request: SignupDTO) {
    return this.write(this.api.create(request), 'Could not create this account.');
  }
}
