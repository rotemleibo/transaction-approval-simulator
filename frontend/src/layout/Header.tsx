import { useTranslation } from 'react-i18next';
import { useAuth } from '../features/auth/AuthContext';
import styles from './Header.module.css';

type HeaderProps = {
  language: 'en' | 'he';
  onChangeLanguage: (lang: 'en' | 'he') => void;
};

export function Header({ language, onChangeLanguage }: HeaderProps) {
  const { t } = useTranslation();
  const { isAuthenticated, username, logout } = useAuth();

  return (
    <header className={styles.header}>
      <div className={styles.brandWrap}>
        <div className={styles.logoGlyph} aria-hidden>
          <span />
          <span />
          <span />
        </div>
        <span className={styles.brandText}>{t('header.brand')}</span>
      </div>

      <div className={styles.controls}>
        {isAuthenticated && (
          <div className={styles.userBadge} title={username ?? undefined}>
            {username}
            <button type="button" className={styles.logoutButton} onClick={logout}>
              {t('header.logout')}
            </button>
          </div>
        )}

        <div className={styles.langToggle} role="group" aria-label="Language toggle">
          <button
            type="button"
            className={language === 'en' ? styles.langActive : styles.langButton}
            onClick={() => onChangeLanguage('en')}
          >
            {t('header.langEng')}
          </button>
          <button
            type="button"
            className={language === 'he' ? styles.langActive : styles.langButton}
            onClick={() => onChangeLanguage('he')}
          >
            {t('header.langHebrew')}
          </button>
        </div>
      </div>
    </header>
  );
}
