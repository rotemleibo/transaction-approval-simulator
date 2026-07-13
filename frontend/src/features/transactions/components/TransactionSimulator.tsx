import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
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
  const { data: regions = [] } = useRegions();
  const simulateMutation = useSimulateTransaction();

  const now = useMemo(() => new Date(), []);
  const [selectedRegionCode, setSelectedRegionCode] = useState('');
  const [date, setDate] = useState(now.toISOString().slice(0, 10));
  const [committedHour, setCommittedHour] = useState(now.getHours());
  const [committedMinute, setCommittedMinute] = useState(now.getMinutes());
  const [draftHour, setDraftHour] = useState(now.getHours());
  const [draftMinute, setDraftMinute] = useState(now.getMinutes());
  const [formError, setFormError] = useState<string | null>(null);

  async function runSimulation(hour: number, minute: number) {
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
      <div className={styles.form}>
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
          hour={draftHour}
          minute={draftMinute}
          onHourChange={(value) => setDraftHour(Math.min(23, Math.max(0, Number.isNaN(value) ? 0 : value)))}
          onMinuteChange={(value) => setDraftMinute(Math.min(59, Math.max(0, Number.isNaN(value) ? 0 : value)))}
          onCancel={() => {
            setDraftHour(committedHour);
            setDraftMinute(committedMinute);
          }}
          onOk={() => {
            setCommittedHour(draftHour);
            setCommittedMinute(draftMinute);
            void runSimulation(draftHour, draftMinute);
          }}
        />

        {formError && <p className={styles.error}>{formError}</p>}

      </div>

      <SimulationResult result={simulateMutation.data ?? null} />
    </section>
  );
}
