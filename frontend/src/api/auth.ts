import apiClient from './client'

export interface RegisterRequest {
  firstName: string
  lastName: string
  email: string
  password: string
  confirmPassword: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface AuthResponse {
  token: string
  email: string
  firstName: string
  lastName: string
}

export interface UserDto {
  id: string
  firstName: string
  lastName: string
  email: string
  isEmailConfirmed: boolean
}

export const register = (data: RegisterRequest) =>
  apiClient.post('/api/auth/register', data)

export const login = (data: LoginRequest) =>
  apiClient.post<AuthResponse>('/api/auth/login', data)

export const confirmEmail = (token: string) =>
  apiClient.post('/api/auth/confirm-email', { token })

export const getMe = () => apiClient.get<UserDto>('/api/auth/me')
