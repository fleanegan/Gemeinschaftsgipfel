import apiClient from './client';

export interface LoginCredentials {
  username: string;
  password: string;
}

export interface SignupData {
  username: string;
  password: string;
  email?: string;
}

export interface LoginResponse {
  token: string;
}

export const authService = {
  async login(credentials: LoginCredentials): Promise<string> {
    const response = await apiClient.post<string>('/api/auth/login', credentials);
    return response.data;
  },

  async signup(data: SignupData): Promise<string> {
    const response = await apiClient.post<string>('/api/auth/signup', data);
    return response.data;
  },

  async rejectMe(): Promise<void> {
    await apiClient.get('/api/auth/rejectme');
  },
};
