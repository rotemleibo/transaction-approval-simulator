import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Header } from '../layout/Header';
import { AuthPanel } from '../features/auth/components/AuthPanel';
import { useAuth } from '../features/auth/AuthContext';
import { useApprovedTransactions } from '../features/transactions/hooks/useApprovedTransactions';
import { TransactionSimulator } from '../features/transactions/components/TransactionSimulator';
import { ApprovedTransactionsCarousel } from '../features/transactions/components/ApprovedTransactionsCarousel';
import type { SimulateTransactionResponse } from '../types/api';
import styles from './App.module.css';

export default function App() {
  const { i18n, t } = useTranslation();
  const { isAuthenticated } = useAuth();

  const language = useMemo<'en' | 'he'>(() => (i18n.language === 'he' ? 'he' : 'en'), [i18n.language]);
  const [lastResult, setLastResult] = useState<SimulateTransactionResponse | null>(null);

  const approvedQuery = useApprovedTransactions(1, 20, isAuthenticated);

  useEffect(() => {
    document.documentElement.lang = language;
    document.documentElement.dir = language === 'he' ? 'rtl' : 'ltr';
    localStorage.setItem('transaction_lang', language);
  }, [language]);

  async function changeLanguage(next: 'en' | 'he') {
    await i18n.changeLanguage(next);
  }

  return (
    <div className={styles.page}>
      <Header language={language} onChangeLanguage={changeLanguage} />

      <main className={styles.main}>
        <section className={styles.hero}>
          <p className={styles.badge}>{t('hero.badge')}</p>
          <h1>{t('hero.title')}</h1>
        </section>

        {!isAuthenticated && (
          <section className={styles.authRow}>
            <AuthPanel />
          </section>
        )}

        <section className={styles.simulatorArea}>
          <TransactionSimulator onResult={setLastResult} />
          <aside className={styles.visualCard}>
            <div className={styles.fakeSiteBar}>
              <span />
              <span />
              <span />
            </div>
            <div className={styles.visualContent}>
              <div className={styles.visualText}>shva</div>
              <div className={styles.deviceMock} />
            </div>
          </aside>
        </section>

        <ApprovedTransactionsCarousel
          transactions={approvedQuery.data?.items ?? []}
          loading={approvedQuery.isLoading}
        />

        {lastResult && <div className={styles.srOnly}>{lastResult.status}</div>}
      </main>
    </div>
  );
}
