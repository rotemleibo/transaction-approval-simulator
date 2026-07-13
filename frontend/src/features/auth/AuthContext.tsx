import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import type { PropsWithChildren } from 'react';
import { login, signup } from '../../services/api/authApi';
import { unauthorizedEventName } from '../../services/api/http';
import { tokenStorage } from './tokenStorage';

type AuthContextValue = {
  username: string | null;
  token: string | null;
  isAuthenticated: boolean;
  signupAsync: (username: string, password: string) => Promise<void>;
  loginAsync: (username: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: PropsWithChildren) {
  const [token, setToken] = useState<string | null>(() => tokenStorage.getToken());
  const [username, setUsername] = useState<string | null>(() => tokenStorage.getUsername());

  useEffect(() => {
    function handleUnauthorized() {
      tokenStorage.clear();
      setToken(null);
      setUsername(null);
    }

    window.addEventListener(unauthorizedEventName, handleUnauthorized);
    return () => {
      window.removeEventListener(unauthorizedEventName, handleUnauthorized);
    };
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      token,
      username,
      isAuthenticated: Boolean(token),
      async signupAsync(nextUsername: string, password: string) {
        const response = await signup({ username: nextUsername, password });
        tokenStorage.setToken(response.token);
        tokenStorage.setUsername(response.username);
        setToken(response.token);
        setUsername(response.username);
      },
      async loginAsync(nextUsername: string, password: string) {
        const response = await login({ username: nextUsername, password });
        tokenStorage.setToken(response.token);
        tokenStorage.setUsername(response.username);
        setToken(response.token);
        setUsername(response.username);
      },
      logout() {
        tokenStorage.clear();
        setToken(null);
        setUsername(null);
      },
    }),
    [token, username],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
