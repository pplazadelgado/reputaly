import styles from './ProgressBar.module.css';

export interface ProgressBarProps {
  value: number;
  max?: number;
  color?: 'navy' | 'blue' | 'green';
  className?: string;
}

export function ProgressBar({ value, max = 100, color = 'navy', className }: ProgressBarProps) {
  const pct = Math.min(100, Math.max(0, (value / max) * 100));

  return (
    <div
      className={[styles.track, className ?? ''].filter(Boolean).join(' ')}
      role="progressbar"
      aria-valuenow={value}
      aria-valuemin={0}
      aria-valuemax={max}
    >
      <div className={[styles.fill, styles[color]].join(' ')} style={{ width: `${pct}%` }} />
    </div>
  );
}
