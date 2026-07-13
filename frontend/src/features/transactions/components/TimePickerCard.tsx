import { useTranslation } from 'react-i18next';
import styles from './TimePickerCard.module.css';

type Props = {
  hour: number;
  minute: number;
  onHourChange: (value: number) => void;
  onMinuteChange: (value: number) => void;
  onCancel: () => void;
  onOk: () => void;
};

export function TimePickerCard({ hour, minute, onHourChange, onMinuteChange, onCancel, onOk }: Props) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === 'he';

  return (
    <div className={`${styles.card} ${isRtl ? styles.rtl : ''}`}>
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
          <button type="button" onClick={onCancel}>
            {t('form.cancel')}
          </button>
          <button type="button" onClick={onOk}>
            {t('form.ok')}
          </button>
        </div>
      </div>
    </div>
  );
}
