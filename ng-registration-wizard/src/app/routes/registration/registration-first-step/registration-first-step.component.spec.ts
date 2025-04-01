import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegistrationFirstStepComponent } from './registration-first-step.component';

describe('RegistrationFirstStepComponent', () => {
  let component: RegistrationFirstStepComponent;
  let fixture: ComponentFixture<RegistrationFirstStepComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegistrationFirstStepComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegistrationFirstStepComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
