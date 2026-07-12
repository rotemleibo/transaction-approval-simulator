import { useTranslation } from 'react-i18next';
import styles from './TimePickerCard.module.css';

type Props = {
  hour: number;
  minute: number;
  onHourChange: (value: number) => void;
  onMinuteChange: (value: number) => void;
};

export function TimePickerCard({ hour, minute, onHourChange, onMinuteChange }: Props) {
  const { t } = useTranslation();

  return (
    <div className={styles.card}>
      <div className={styles.title}>{t('form.timeLabel')}</div>

      <div className={styles.row}>
        <div className={styles.timeBox}>
          <input
            type="number"
            min={0}
            max={23}
            value={hour}
            onChange={(e) => onHourChange(Number(e.target.value))}
          />
          <span>{t('form.hour')}</span>
        </div>

        <span className={styles.separator}>:</span>

        <div className={styles.timeBox}>
          <input
            type="number"
            min={0}
            max={59}
            value={minute}
            onChange={(e) => onMinuteChange(Number(e.target.value))}
          />
          <span>{t('form.minute')}</span>
        </div>
      </div>

      <div className={styles.footer}>
        <span className={styles.clock}>◷</span>
        <div className={styles.actions}>
          <button type="button">{t('form.cancel')}</button>
          <button type="button">{t('form.ok')}</button>
        </div>
      </div>
    </div>
  );
}
