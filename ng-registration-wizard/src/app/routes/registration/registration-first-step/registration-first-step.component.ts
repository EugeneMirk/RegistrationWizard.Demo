import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { catchError, of } from 'rxjs';
import { CommonModule } from '@angular/common';
import { AccountService } from '../../../core/services/account.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-registration-first-step',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatIconModule
  ],
  templateUrl: './registration-first-step.component.html',
  styleUrls: ['./registration-first-step.component.scss']
})
export class RegistrationFirstStepComponent {
  @Output() next = new EventEmitter<any>();
  @Input() initialData: any;

  emailError = '';
  isLoading = false;

  form = new FormGroup({
    email: new FormControl<string>('', {
      validators: [
        Validators.required,
        this.validateEmail()
      ],
      nonNullable: true
    }),
    password: new FormControl<string>('', {
      validators: [
        Validators.required,
        this.validatePassword()
      ],
      nonNullable: true
    }),
    confirmPassword: new FormControl<string>('', {
      validators: [Validators.required],
      nonNullable: true
    }),
    agreeTerms: new FormControl<boolean>(false, {
      validators: [Validators.requiredTrue],
      nonNullable: true
    })
  }, { validators: this.matchPasswords() });

  constructor(
    private accountService: AccountService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    if (this.initialData) {
      this.form.patchValue(this.initialData);
    }
  }

  private validateEmail(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const email = control.value;
      if (!email) return null;
      const regex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
      return regex.test(email) ? null : { invalidEmail: true };
    };
  }

  private validatePassword(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const password = control.value;
      if (!password) return null;
      const hasLetter = /[A-Za-z]/.test(password);
      const hasNumber = /\d/.test(password);
      return hasLetter && hasNumber ? null : { invalidPassword: true };
    };
  }

  private matchPasswords(): ValidatorFn {
    return (formGroup: AbstractControl): ValidationErrors | null => {
      const passwordControl = formGroup.get('password');
      const confirmPasswordControl = formGroup.get('confirmPassword');

      if (!passwordControl || !confirmPasswordControl) {
        return null;
      }

      if (passwordControl.value !== confirmPasswordControl.value) {
        confirmPasswordControl.setErrors({ mismatch: true });
        return { mismatch: true };
      } else {
        confirmPasswordControl.setErrors(null);
        return null;
      }
    };
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.emailError = '';

    this.accountService.checkEmailAvailable(this.form.value.email!)
      .pipe(catchError(() => of(false)))
      .subscribe({
        next: (isAvailable) => {
          this.isLoading = false;
          if (isAvailable) {
            this.next.emit(this.form.value);
          } else {
            this.snackBar.open('This email is already registered', 'Close', {
              duration: 5000,
              panelClass: ['error-snackbar']
            });
          }
        },
        error: () => {
          this.isLoading = false;
          this.snackBar.open('Error checking email availability', 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
      });
  }
}