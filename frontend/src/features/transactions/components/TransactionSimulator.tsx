import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../auth/AuthContext';
import { useRegions } from '../hooks/useRegions';
import { useSimulateTransaction } from '../hooks/useSimulateTransaction';
import type { SimulateTransactionResponse } from '../../../types/api';
import { RegionCombobox } from './RegionCombobox';
import { TimePickerCard } from './TimePickerCard';
import { SimulationResult } from './SimulationResult';
import styles from './TransactionSimulator.module.css';

type Props = {
  onResult: (result: SimulateTransactionResponse | null) => void;
};

export function TransactionSimulator({ onResult }: Props) {
  const { t } = useTranslation();
  const { isAuthenticated } = useAuth();
  const { data: regions = [] } = useRegions();
  const simulateMutation = useSimulateTransaction();

  const now = useMemo(() => new Date(), []);
  const [selectedRegionCode, setSelectedRegionCode] = useState('');
  const [date, setDate] = useState(now.toISOString().slice(0, 10));
  const [hour, setHour] = useState(now.getHours());
  const [minute, setMinute] = useState(now.getMinutes());
  const [formError, setFormError] = useState<string | null>(null);

  const canSubmit =
    isAuthenticated &&
    selectedRegionCode.length > 0 &&
    date.length > 0 &&
    hour >= 0 &&
    hour <= 23 &&
    minute >= 0 &&
    minute <= 59 &&
    !simulateMutation.isPending;

  async function onSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();

    if (!selectedRegionCode) {
      setFormError(t('form.validationRegion'));
      return;
    }

    if (!date) {
      setFormError(t('form.validationDate'));
      return;
    }

    setFormError(null);

    const [year, month, day] = date.split('-').map(Number);
    const submittedAt = new Date(year, month - 1, day, hour, minute, 0, 0).toISOString();

    try {
      const response = await simulateMutation.mutateAsync({
        regionCode: selectedRegionCode,
        submittedAt,
      });
      onResult(response);
    } catch {
      onResult(null);
      setFormError(t('errors.generic'));
    }
  }

  return (
    <section className={styles.wrap}>
      <form className={styles.form} onSubmit={onSubmit}>
        <RegionCombobox
          regions={regions}
          selectedRegionCode={selectedRegionCode}
          onSelect={setSelectedRegionCode}
        />

        <label className={styles.dateLabel}>
          {t('form.dateLabel')}
          <input type="date" value={date} onChange={(e) => setDate(e.target.value)} required />
        </label>

        <TimePickerCard
          hour={hour}
          minute={minute}
          onHourChange={(value) => setHour(Math.min(23, Math.max(0, Number.isNaN(value) ? 0 : value)))}
          onMinuteChange={(value) => setMinute(Math.min(59, Math.max(0, Number.isNaN(value) ? 0 : value)))}
        />

        {formError && <p className={styles.error}>{formError}</p>}

        <button className={styles.submit} disabled={!canSubmit} type="submit">
          {simulateMutation.isPending ? t('form.simulating') : t('form.simulate')}
        </button>
      </form>

      <SimulationResult result={simulateMutation.data ?? null} />
    </section>
  );
}
