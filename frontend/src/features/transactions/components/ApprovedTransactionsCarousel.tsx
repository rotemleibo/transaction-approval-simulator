import { useRef } from 'react';
import { useTranslation } from 'react-i18next';
import type { ApprovedTransaction } from '../../../types/api';
import styles from './ApprovedTransactionsCarousel.module.css';

type Props = {
  transactions: ApprovedTransaction[];
  loading: boolean;
};

export function ApprovedTransactionsCarousel({ transactions, loading }: Props) {
  const { t } = useTranslation();
  const listRef = useRef<HTMLDivElement>(null);

  function scrollByAmount(offset: number) {
    listRef.current?.scrollBy({ left: offset, behavior: 'smooth' });
  }

  return (
    <section className={styles.section}>
      <h2>{t('approvedList.title')}</h2>
      {loading ? (
        <div className={styles.loadingGrid}>
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className={styles.skeleton} />
          ))}
        </div>
      ) : transactions.length === 0 ? (
        <p className={styles.empty}>{t('approvedList.empty')}</p>
      ) : (
        <div className={styles.carouselWrap}>
          <button type="button" onClick={() => scrollByAmount(-340)} className={styles.arrow}>
            ←
          </button>
          <div className={styles.track} ref={listRef}>
            {transactions.map((tx) => {
              const localDate = new Date(tx.localTransactionTime);
              return (
                <article key={tx.id} className={styles.card}>
                  <p className={styles.time}>
                    {t('approvedList.time')}: {localDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </p>
                  <p>
                    {t('approvedList.timezone')}: {tx.regionName}
                  </p>
                </article>
              );
            })}
          </div>
          <button type="button" onClick={() => scrollByAmount(340)} className={styles.arrow}>
            →
          </button>
        </div>
      )}
    </section>
  );
}
