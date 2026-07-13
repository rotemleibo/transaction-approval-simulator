import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../AuthContext';
import styles from './AuthPanel.module.css';

type Mode = 'login' | 'signup';

export function AuthPanel() {
  const { t } = useTranslation();
  const { loginAsync, signupAsync } = useAuth();

  const [mode, setMode] = useState<Mode>('login');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      if (mode === 'login') {
        await loginAsync(username, password);
      } else {
        await signupAsync(username, password);
      }
      setUsername('');
      setPassword('');
    } catch {
      setError(t('auth.error'));
    } finally {
      setLoading(false);
    }
  }

  return (
    <form className={styles.panel} onSubmit={submit}>
      <div className={styles.modeToggle} role="group" aria-label="Authentication mode">
        <button
          type="button"
          className={mode === 'login' ? styles.modeActive : styles.modeButton}
          disabled={loading}
          onClick={() => setMode('login')}
        >
          {t('auth.login')}
        </button>
        <button
          type="button"
          className={mode === 'signup' ? styles.modeActive : styles.modeButton}
          disabled={loading}
          onClick={() => setMode('signup')}
        >
          {t('auth.signup')}
        </button>
      </div>

      <h2 className={styles.title}>{mode === 'login' ? t('auth.loginTitle') : t('auth.signupTitle')}</h2>
      <p className={styles.subtitle}>{mode === 'login' ? t('auth.loginSubtitle') : t('auth.signupSubtitle')}</p>

      <label className={styles.field}>
        <span>{t('auth.username')}</span>
        <input
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          placeholder={t('auth.username')}
          autoComplete="username"
          required
          minLength={3}
        />
      </label>

      <label className={styles.field}>
        <span>{t('auth.password')}</span>
        <input
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder={t('auth.password')}
          type="password"
          autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
          required
          minLength={6}
        />
      </label>

      {error && <p className={styles.error}>{error}</p>}

      <div className={styles.actions}>
        <button type="submit" disabled={loading} className={styles.primaryAction}>
          {mode === 'login' ? t('auth.login') : t('auth.signup')}
        </button>
      </div>
    </form>
  );
}
