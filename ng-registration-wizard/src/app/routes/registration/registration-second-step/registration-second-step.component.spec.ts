import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegistrationSecondStepComponent } from './registration-second-step.component';

describe('RegistrationSecondStepComponent', () => {
  let component: RegistrationSecondStepComponent;
  let fixture: ComponentFixture<RegistrationSecondStepComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegistrationSecondStepComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegistrationSecondStepComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
