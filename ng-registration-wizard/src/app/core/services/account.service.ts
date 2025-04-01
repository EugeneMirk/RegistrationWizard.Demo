import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RegistrationData } from './models/registration-data.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  constructor(private http: HttpClient) {}
    private server = 'http://localhost:5008'

  checkEmailAvailable(email: string): Observable<boolean> {
    return this.http.get<boolean>(this.server + '/api/accounts/check-email', {
      params: { login: email }
    });
  }

  register(data: RegistrationData): Observable<any> {
    return this.http.post(this.server + '/api/accounts/register', data);
  }
}
