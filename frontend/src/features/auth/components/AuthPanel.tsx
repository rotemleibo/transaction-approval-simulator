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
      <h3>{t('auth.title')}</h3>
      <label>
        {t('auth.username')}
        <input value={username} onChange={(e) => setUsername(e.target.value)} required minLength={3} />
      </label>
      <label>
        {t('auth.password')}
        <input
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          type="password"
          required
          minLength={6}
        />
      </label>

      {error && <p className={styles.error}>{error}</p>}

      <div className={styles.actions}>
        <button type="submit" disabled={loading}>
          {mode === 'login' ? t('auth.login') : t('auth.signup')}
        </button>
        <button
          type="button"
          className={styles.secondary}
          disabled={loading}
          onClick={() => setMode(mode === 'login' ? 'signup' : 'login')}
        >
          {mode === 'login' ? t('auth.signup') : t('auth.login')}
        </button>
      </div>
    </form>
  );
}
