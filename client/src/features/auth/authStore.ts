import { create } from 'zustand';
import { login as loginApi } from './authApi';

const STORAGE_KEY = 'llmcoord-auth';

interface StoredAuth {
  token: string;
  userId: string;
  tenantId: string;
  role: string;
  fullName: string;
}

interface AuthState {
  token: string | null;
  userId: string | null;
  tenantId: string | null;
  role: string | null;
  fullName: string | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  loadFromStorage: () => void;
  isAuthenticated: () => boolean;
}

function persistAuth(data: StoredAuth): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
}

function clearAuth(): void {
  localStorage.removeItem(STORAGE_KEY);
}

export const useAuthStore = create<AuthState>((set, get) => ({
  token: null,
  userId: null,
  tenantId: null,
  role: null,
  fullName: null,

  login: async (email: string, password: string) => {
    const response = await loginApi(email, password);
    const auth: StoredAuth = {
      token: response.token,
      userId: response.userId,
      tenantId: response.tenantId,
      role: response.role,
      fullName: response.fullName,
    };
    persistAuth(auth);
    set(auth);
  },

  logout: () => {
    clearAuth();
    set({
      token: null,
      userId: null,
      tenantId: null,
      role: null,
      fullName: null,
    });
  },

  loadFromStorage: () => {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return;
    try {
      const parsed = JSON.parse(raw) as StoredAuth;
      set(parsed);
    } catch {
      clearAuth();
    }
  },

  isAuthenticated: () => Boolean(get().token),
}));
