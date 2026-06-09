import { httpClient } from '../../shared/api/httpClient';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  userId: string;
  tenantId: string;
  role: string;
  fullName: string;
}

export async function login(email: string, password: string): Promise<LoginResponse> {
  const { data } = await httpClient.post<LoginResponse>('/api/auth/login', {
    email,
    password,
  } satisfies LoginRequest);
  return data;
}
