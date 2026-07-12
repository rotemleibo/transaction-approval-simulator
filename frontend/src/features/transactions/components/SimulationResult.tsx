import { useTranslation } from 'react-i18next';
import type { SimulateTransactionResponse } from '../../../types/api';
import styles from './SimulationResult.module.css';

type Props = {
  result: SimulateTransactionResponse | null;
};

export function SimulationResult({ result }: Props) {
  const { t } = useTranslation();

  if (!result) {
    return <section className={styles.placeholder}>{t('result.placeholder')}</section>;
  }

  const approved = result.status === 'Approved';

  return (
    <section className={approved ? styles.approved : styles.rejected}>
      <h3>{t('result.title')}</h3>
      <p className={styles.badge}>{approved ? t('result.approved') : t('result.rejected')}</p>
      <p>
        <strong>{t('result.region')}:</strong> {result.regionName}
      </p>
      <p>
        <strong>{t('result.timezone')}:</strong> {result.timeZoneId}
      </p>
      <p>
        <strong>{t('result.submittedUtc')}:</strong> {new Date(result.submittedUtc).toISOString()}
      </p>
      <p>
        <strong>{t('result.localTime')}:</strong> {new Date(result.localTransactionTime).toLocaleString()}
      </p>
      <p className={styles.reason}>{result.reason}</p>
    </section>
  );
}
