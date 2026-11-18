import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Urls } from '../../constants/urls';
import { LoginDTO } from '../../DTO/auth/login.dto';
import { UserDTO } from '../../DTO/auth/user.dto';
import { ChangePasswordDTO } from '../../DTO/auth/change-password.dto';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly url = `${Urls.api}/auth`;

  login(request: LoginDTO) {
    return this.http.post<void>(`${this.url}/login`, request);
  }

  me() {
    return this.http.get<UserDTO>(`${this.url}/me`);
  }

  refresh() {
    return this.http.post<void>(`${this.url}/refresh-token`, {});
  }

  logout() {
    return this.http.post<void>(`${this.url}/logout`, {});
  }

  changePassword(request: ChangePasswordDTO) {
    return this.http.post<void>(`${this.url}/change-password`, request);
  }
}
