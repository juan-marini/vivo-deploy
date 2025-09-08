export interface LoginRequest {
  email: string;
  senha: string;
  lembrarMe: boolean;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  token?: string;
  refreshToken?: string;
  usuario?: Usuario;
  expiresAt?: Date;
}

export interface Usuario {
  id: number;
  email: string;
  nomeCompleto: string;
  perfil: string;
  avatarUrl?: string;
  primeiroAcesso: boolean;
  departamento?: string;
  cargo?: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}