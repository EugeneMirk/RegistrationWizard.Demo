import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CountryService } from '../../../core/services/country.service';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-registration-second-step',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './registration-second-step.component.html',
  styleUrls: ['./registration-second-step.component.scss']
})
export class RegistrationSecondStepComponent {
  @Output() back = new EventEmitter<any>();
  @Output() save = new EventEmitter<any>();
  @Input() initialData: any;

  countries = signal<{ id: number; name: string }[]>([]);
  provinces = signal<{ id: number; name: string }[]>([]);
  isLoading = false;

  form = new FormGroup({
    country: new FormControl<number | null>(null, Validators.required),
    province: new FormControl<number | null>(null, Validators.required),
  });

  constructor(private countryService: CountryService) {
    this.loadCountries();
  }
  
  compareById(item1: any, item2: any): boolean {
    return item1 && item2 ? item1.id === item2.id : item1 === item2;
  }

  ngOnInit(): void {
    if (this.initialData) {
      this.form.patchValue(this.initialData);
      if (this.initialData.country) {
        this.loadProvinces();
      }
    }
  }

  loadCountries(): void {
    this.isLoading = true;
    this.countryService.getCountries().subscribe({
      next: (data) => {
        this.countries.set(data);
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadProvinces(): void {
    const countryId = this.form.value.country;
    if (countryId) {
      this.isLoading = true;
      this.countryService.getProvinces(countryId).subscribe({
        next: (data) => {
          this.provinces.set(data);
          this.isLoading = false;
          this.form.controls.province.setValue(null);
        },
        error: () => {
          this.isLoading = false;
        }
      });
    }
  }

  onCountryChange(): void {
    this.loadProvinces();
  }

  goBack(): void {
    this.back.emit(this.form.value);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.save.emit(this.form.value);
  }
}