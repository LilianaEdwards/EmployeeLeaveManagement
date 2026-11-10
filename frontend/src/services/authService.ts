import api from './api'

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  accessToken: string
  expiresAtUtc: string
  employeeId: string
  fullName: string
  email: string
  role: 'Employee' | 'HOD' | 'HR'
}

export const authService = {
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await api.post<LoginResponse>(
      '/auth/login',
      credentials,
    )

    return response.data
  },

  logout(): void {
    sessionStorage.removeItem('accessToken')
    sessionStorage.removeItem('user')
  },
}