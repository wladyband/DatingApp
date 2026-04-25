import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { Nav } from "../layout/nav/nav";

type User = {
  id: string;
  email: string;
  displayName: string;
};

type ApiResponse<T> = {
  success: boolean;
  data: T;
};


@Component({
  selector: 'app-root',
  imports: [Nav],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private http = inject(HttpClient);
  protected title = 'Dating app';
  protected users = signal<User[]>([]);

  async ngOnInit() {
    this.users.set(await this.getUsers());
  }
  async getUsers(): Promise<User[]> {
    try {
      const response = await lastValueFrom(
        this.http.get<ApiResponse<User[]>>('http://localhost:5001/api/users')
      );

      return response.data ?? [];
    } catch (error) {
      console.error('Error fetching users:', error);
      return [];
    }
  }

}
