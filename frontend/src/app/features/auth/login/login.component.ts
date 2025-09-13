import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { BrowserStorageService } from '../../../core/services/browser-storage.service';
import { UserProfileService } from '../../../core/services/user-profile.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  isLoading = false;
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService,
    private storage: BrowserStorageService,
    private userProfileService: UserProfileService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      senha: ['', [Validators.required, Validators.minLength(6)]],
      lembrarMe: [false]
    });
  }

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }

    const savedEmail = this.storage.getItem('rememberedEmail', 'local');
    if (savedEmail) {
      this.loginForm.patchValue({
        email: savedEmail,
        lembrarMe: true
      });
    }
  }


  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.markFormGroupTouched(this.loginForm);
      return;
    }

    this.isLoading = true;
    const { email, senha, lembrarMe } = this.loginForm.value;

    this.authService.login({ email, senha, lembrarMe }).subscribe({
      next: (response) => {
        if (response.success) {
          if (lembrarMe) {
            this.storage.setItem('rememberedEmail', email, 'local');
          } else {
            this.storage.removeItem('rememberedEmail', 'local');
          }

          this.toastr.success('Login realizado com sucesso!', 'Bem-vindo');

          // Aguardar um momento para o usuário ser definido e redirecionar para rota baseada no perfil
          setTimeout(() => {
            const defaultRoute = this.userProfileService.getDefaultRoute();
            this.router.navigate([defaultRoute]);
          }, 100);
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.toastr.error(
          error.error?.message || 'Erro ao realizar login',
          'Erro'
        );
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  // 🔹 Método para "Esqueci minha senha"
  onForgotPassword(): void {
    const email = this.loginForm.get('email')?.value;

    if (!email) {
      this.toastr.warning('Digite seu e-mail para recuperar a senha.', 'Atenção');
      return;
    }

    this.isLoading = true;
    this.authService.forgotPassword(email).subscribe({
      next: () => {
        this.toastr.success('Instruções de recuperação enviadas para seu e-mail.', 'Recuperação de Senha');
      },
      error: (error) => {
        this.toastr.error(
          error.error?.message || 'Erro ao solicitar recuperação de senha',
          'Erro'
        );
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  get emailControl() {
    return this.loginForm.get('email');
  }

  get senhaControl() {
    return this.loginForm.get('senha');
  }
}
