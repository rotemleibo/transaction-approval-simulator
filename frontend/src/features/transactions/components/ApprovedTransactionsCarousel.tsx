import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { ApprovedTransaction } from '../../../types/api';
import styles from './ApprovedTransactionsCarousel.module.css';

type Props = {
  transactions: ApprovedTransaction[];
  loading: boolean;
  hasNextPage: boolean;
  isFetchingNextPage: boolean;
  onFetchNextPage: () => void;
};

export function ApprovedTransactionsCarousel({
  transactions,
  loading,
  hasNextPage,
  isFetchingNextPage,
  onFetchNextPage,
}: Props) {
  const { t } = useTranslation();
  const trackRef = useRef<HTMLDivElement>(null);
  const [index, setIndex] = useState(0);
  const pendingAdvanceRef = useRef(false);

  const atStart = index <= 0;
  const atEnd = index >= transactions.length - 1;

  // After more items arrive following an end-of-list fetch, advance to the first new card.
  useEffect(() => {
    if (pendingAdvanceRef.current && index < transactions.length - 1) {
      pendingAdvanceRef.current = false;
      setIndex((i) => i + 1);
    }
  }, [transactions.length, index]);

  useEffect(() => {
    const track = trackRef.current;
    const card = track?.children[index] as HTMLElement | undefined;
    if (track && card) {
      // scrollBy with a rect delta works correctly in both LTR and RTL layouts.
      const delta = card.getBoundingClientRect().left - track.getBoundingClientRect().left;
      track.scrollBy({ left: delta, behavior: 'smooth' });
    }
  }, [index, transactions.length]);

  function goPrev() {
    setIndex((i) => Math.max(0, i - 1));
  }

  function goNext() {
    if (!atEnd) {
      setIndex((i) => i + 1);
    } else if (hasNextPage && !isFetchingNextPage) {
      pendingAdvanceRef.current = true;
      onFetchNextPage();
    }
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
          <button
            type="button"
            onClick={goPrev}
            className={styles.arrow}
            disabled={atStart}
            aria-label={t('approvedList.prevPage')}
          >
            ←
          </button>
          <div className={styles.track} ref={trackRef}>
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
          <button
            type="button"
            onClick={goNext}
            className={styles.arrow}
            disabled={(atEnd && !hasNextPage) || isFetchingNextPage}
            aria-label={t('approvedList.nextPage')}
          >
            →
          </button>
        </div>
      )}
    </section>
  );
}
