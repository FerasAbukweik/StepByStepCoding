import { SignupDTO } from '../../DTO/auth/signup.dto';
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { UserLookupDTO } from '../../DTO/auth/user-lookup.dto';

@Injectable({ providedIn: 'root' })
export class UsersApiService {
  private readonly http = inject(HttpClient);

  search(role: 'Instructor' | 'Student', search = '') {
    return this.http.get<UserLookupDTO[]>('/api/users', { params: { role, search } });
  }
  create(request: SignupDTO) {
    return this.http.post<void>('/api/auth/signup', request);
  }
}
