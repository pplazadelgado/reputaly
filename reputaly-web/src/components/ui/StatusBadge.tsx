import styles from './StatusBadge.module.css';

export type ReviewStatus = 'pending' | 'auto' | 'escalated' | 'replied' | 'error';

const LABELS: Record<ReviewStatus, string> = {
  pending: 'Pendiente',
  auto: 'Auto-respondida',
  escalated: 'Escalada',
  replied: 'Respondida',
  error: 'Error',
};

export interface StatusBadgeProps {
  status: ReviewStatus;
}

export function StatusBadge({ status }: StatusBadgeProps) {
  return (
    <span className={[styles.badge, styles[status]].join(' ')}>
      <span className={styles.dot} aria-hidden="true" />
      {LABELS[status]}
    </span>
  );
}
