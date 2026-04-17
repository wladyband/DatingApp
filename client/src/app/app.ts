import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { lastValueFrom } from 'rxjs';


@Component({
  selector: 'app-root',
  imports: [],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private http = inject(HttpClient);
  protected title = 'Dating app';
  protected users = signal<any>([]);

  async ngOnInit() {
    this.users.set(await this.getUsers());
  }
  async getUsers() {
    try {
      return await lastValueFrom(this.http.get('http://localhost:5001/api/users'));
    } catch (error) {
      console.error('Error fetching users:', error);
      return [];
    }
  }

}
