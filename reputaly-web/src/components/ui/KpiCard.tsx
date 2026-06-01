import { ArrowUpRight, ArrowDownRight } from 'lucide-react';
import styles from './KpiCard.module.css';

export interface KpiCardProps {
  label: string;
  value: string;
  trend?: number;
  subtitle?: string;
  className?: string;
}

export function KpiCard({ label, value, trend, subtitle = 'vs. mes anterior', className }: KpiCardProps) {
  const isUp = trend !== undefined && trend >= 0;
  const trendAbs = trend !== undefined ? Math.abs(trend) : undefined;

  return (
    <div className={[styles.card, className ?? ''].filter(Boolean).join(' ')}>
      <div className={styles.label}>{label}</div>
      <div className={styles.valueRow}>
        <div className={styles.value}>{value}</div>
        {trendAbs !== undefined && (
          <span className={[styles.trend, isUp ? styles.up : styles.down].join(' ')}>
            {isUp ? (
              <ArrowUpRight size={12} strokeWidth={2} aria-hidden="true" />
            ) : (
              <ArrowDownRight size={12} strokeWidth={2} aria-hidden="true" />
            )}
            {trendAbs.toFixed(1)}%
          </span>
        )}
      </div>
      {subtitle && <div className={styles.subtitle}>{subtitle}</div>}
    </div>
  );
}
