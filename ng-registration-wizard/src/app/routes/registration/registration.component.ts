import { Component, signal } from '@angular/core';
import { NgIf } from '@angular/common';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { AccountService } from '../../core/services/account.service';
import { RegistrationFirstStepComponent } from './registration-first-step/registration-first-step.component';
import { RegistrationSecondStepComponent } from './registration-second-step/registration-second-step.component';
import { MatProgressSpinner } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-registration',
  standalone: true,
  imports: [
    RegistrationFirstStepComponent,
    RegistrationSecondStepComponent,
    NgIf,
    MatProgressSpinner],
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.scss']
})
export class RegistrationComponent {
  step = signal(1);
  firstStepData: any;
  secondStepData: any;
  isLoading = false;

  constructor(
    private accountService: AccountService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  onFirstStepSubmit(data: any) {
    this.firstStepData = data;
    this.step.set(2);
  }

  onBack(data: any) {
    this.secondStepData = data;
    this.step.set(1);
  }

  onSave(data: any) {
    this.secondStepData = data;
    this.registerUser();
  }

  registerUser() {
    this.isLoading = true;
    
    const registrationData = {
      email: this.firstStepData.email,
      password: this.firstStepData.password,
      confirmPassword:this.firstStepData.confirmPassword,
      countryId: this.secondStepData.country,
      provinceId: this.secondStepData.province,
      agreeToTerms: this.firstStepData.agreeTerms
    };

    this.accountService.register(registrationData).subscribe({
      next: () => {
        this.snackBar.open('Registration successful!', 'Close', { duration: 3000 });
        this.router.navigate(['/login']);
        this.isLoading = false;
      },
      error: (err: any) => {
        this.isLoading = false;
        this.handleError(err);
        this.isLoading = false;
      }
    });
  }

  private handleError(err: any) {
    let message = 'Registration failed';
    
    if (err.status === 400) message = 'Invalid data';
    if (err.status === 409) message = 'Email already exists';
    if (err.status >= 500) message = 'Server error';

    this.snackBar.open(message, 'Close', { 
      duration: 5000,
      panelClass: 'error-snackbar'
    });
  }
}