import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { UserStore } from '../../store/user.store';

type ModalMode = 'signin' | 'register';

@Component({
  selector: 'app-user-selector',
  imports: [ReactiveFormsModule],
  templateUrl: './user-selector.component.html',
  styleUrl: './user-selector.component.scss',
})
export class UserSelectorComponent {
  store = inject(UserStore);
  private fb = inject(FormBuilder);

  dropdownOpen = signal(false);
  modalOpen = signal(false);
  mode = signal<ModalMode>('signin');
  loading = signal(false);
  error = signal<string | null>(null);

  signInForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  registerForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    firstName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
    lastName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  get fullName() {
    const u = this.store.currentUser();
    return u ? `${u.firstName} ${u.lastName}` : '';
  }

  initials() {
    const u = this.store.currentUser();
    return u ? `${u.firstName[0]}${u.lastName[0]}`.toUpperCase() : '';
  }

  openModal(mode: ModalMode = 'signin') {
    this.mode.set(mode);
    this.error.set(null);
    this.signInForm.reset();
    this.registerForm.reset();
    this.modalOpen.set(true);
  }

  closeModal() {
    this.modalOpen.set(false);
  }

  switchMode(mode: ModalMode) {
    this.mode.set(mode);
    this.error.set(null);
    this.signInForm.reset();
    this.registerForm.reset();
  }

  submitSignIn() {
    if (this.signInForm.invalid) {
      this.signInForm.markAllAsTouched();
      return;
    }
    const { email, password } = this.signInForm.value;
    this.loading.set(true);
    this.error.set(null);

    this.store.signIn(email!, password!).subscribe({
      next: () => { this.loading.set(false); this.closeModal(); },
      error: () => { this.loading.set(false); this.error.set('Invalid email or password.'); },
    });
  }

  submitRegister() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }
    const { email, firstName, lastName, password } = this.registerForm.value;
    this.loading.set(true);
    this.error.set(null);

    this.store.register(email!, firstName!, lastName!, password!).subscribe({
      next: () => { this.loading.set(false); this.closeModal(); },
      error: () => { this.loading.set(false); this.error.set('Registration failed. The email may already be taken.'); },
    });
  }
}
