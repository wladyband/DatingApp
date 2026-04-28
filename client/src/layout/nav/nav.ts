import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../core/services/account-service';

@Component({
  selector: 'app-nav',
  imports: [FormsModule],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  private accountService = inject(AccountService);
  protected creds: any = {}
  protected loggedIn = signal(false);

  login() {
    this.accountService.login(this.creds).subscribe({
      next: (response) => {
        console.log('Login successful:', response);
        this.loggedIn.set(true);
        this.creds = {}; // Clear the credentials after successful login
      },
      error: (error) => {
        console.error('Login failed:', error);
      }
    });
  }
  logout() {
    this.loggedIn.set(false);
  }
}
