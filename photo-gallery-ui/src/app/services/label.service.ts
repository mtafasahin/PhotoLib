import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Label } from '../gallery/gallery.component';

@Injectable({
  providedIn: 'root'
})
export class LabelService {
  private apiUrl = 'api/Image/labels';

  constructor(private http: HttpClient) {}

  // Label'ları getir
  getLabels(): Observable<any[]> {
    return this.http.get<Label[]>(this.apiUrl);
  }

  // Yeni label ekle
  addLabel(label: { name: string }): Observable<any> {
    return this.http.post(this.apiUrl, label);
  }
}
