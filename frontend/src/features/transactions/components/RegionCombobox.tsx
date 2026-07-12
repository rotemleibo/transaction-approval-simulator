import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { Region } from '../../../types/api';
import styles from './RegionCombobox.module.css';

type Props = {
  regions: Region[];
  selectedRegionCode: string;
  onSelect: (regionCode: string) => void;
};

export function RegionCombobox({ regions, selectedRegionCode, onSelect }: Props) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState('');

  const filtered = useMemo(() => {
    const value = search.trim().toLowerCase();
    if (!value) {
      return regions;
    }
    return regions.filter((r) => r.name.toLowerCase().includes(value));
  }, [regions, search]);

  const selectedRegion = regions.find((r) => r.code === selectedRegionCode);

  return (
    <div className={styles.wrap}>
      <label className={styles.label}>{t('form.regionLabel')}</label>
      <div className={styles.inputWrap}>
        <input
          value={search || selectedRegion?.name || ''}
          onChange={(e) => {
            setSearch(e.target.value);
            setOpen(true);
          }}
          onFocus={() => setOpen(true)}
          placeholder={t('form.regionSearch')}
        />
        <button
          type="button"
          aria-label="clear region"
          onClick={() => {
            setSearch('');
            onSelect('');
          }}
        >
          ×
        </button>
      </div>

      {open && (
        <div className={styles.menu}>
          {filtered.map((region) => (
            <button
              type="button"
              key={region.code}
              onClick={() => {
                onSelect(region.code);
                setSearch(region.name);
                setOpen(false);
              }}
            >
              {region.name}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
