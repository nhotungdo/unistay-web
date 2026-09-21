import { useCallback, useSyncExternalStore } from 'react';
import api from '@/lib/api';

/**
 * Full page navigation to /login. A hard reload is intentional here: it
 * clears every in-memory state (SignalR, caches) alongside the auth store.
 * The indirection through location.assign keeps the Next.js lint rule happy
 * while preserving that behavior.
 */
function hardResetToLogin() {
  window.location.assign('/login');
}

export interface User {
  id: string;
  fullName: string;
  email: string;
  avatarUrl?: string;
  roles?: string[];
  isOnboardingComplete?: boolean;
}

/**
 * Module-level auth store backed by localStorage.
 *
 * Using useSyncExternalStore keeps rendering hydration-safe: the server
 * snapshot is always `null`, and the client switches to the localStorage
 * snapshot right after mount — without setState-in-effect cascades.
 */
let currentUser: User | null = null;
let initialized = false;
const listeners = new Set<() => void>();

function init() {
  if (initialized || typeof window === 'undefined') return;
  initialized = true;
  try {
    const storedUser = localStorage.getItem('user');
    const storedToken = localStorage.getItem('token');
    if (storedUser && storedToken) {
      currentUser = JSON.parse(storedUser) as User;
    }
  } catch {
    localStorage.removeItem('user');
    localStorage.removeItem('token');
  }
}

function emit() {
  listeners.forEach((listener) => listener());
}

function subscribe(listener: () => void): () => void {
  init();
  listeners.add(listener);
  return () => listeners.delete(listener);
}

function getSnapshot(): User | null {
  return currentUser;
}

function getServerSnapshot(): User | null {
  return null;
}

function setUser(user: User | null) {
  currentUser = user;
  emit();
}

export function useAuth() {
  const user = useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot);
  const mounted = useSyncExternalStore(
    subscribe,
    () => true,
    () => false
  );

  // True during SSR and the hydration render; false right after mount.
  const loading = !mounted;

  const login = useCallback((token: string, userData: User) => {
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(userData));
    init();
    setUser(userData);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
    hardResetToLogin();
  }, []);

  const checkAuth = useCallback(async () => {
    try {
      const res = await api.get('/account/me');
      if (res.data) {
        localStorage.setItem('user', JSON.stringify(res.data));
        setUser(res.data);
      }
    } catch {
      logout();
    }
  }, [logout]);

  return { user, loading, login, logout, checkAuth };
}
