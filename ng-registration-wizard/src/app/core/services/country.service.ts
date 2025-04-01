import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
@Injectable({ providedIn: 'root' })
export class CountryService {
  private http = inject(HttpClient);
  private server = 'http://localhost:5008'

  getCountries(): Observable<{ id: number; name: string }[]> {
    return this.http.get<{ id: number; name: string }[]>(this.server + '/api/countries');
  }

  getProvinces(countryId: number): Observable<{ id: number; name: string }[]> {
    return this.http.get<{ id: number; name: string }[]>(this.server + `/api/countries/${countryId}/provinces`);
  }
}