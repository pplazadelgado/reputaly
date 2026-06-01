import styles from './StarBars.module.css';

export interface StarBarsProps {
  distribution: [number, number, number, number, number];
}

export function StarBars({ distribution }: StarBarsProps) {
  const total = distribution.reduce((s, v) => s + v, 0) || 1;

  return (
    <div className={styles.root} aria-label="Distribución de valoraciones">
      {[5, 4, 3, 2, 1].map((star) => {
        const count = distribution[star - 1];
        const pct = (count / total) * 100;
        return (
          <div key={star} className={styles.row}>
            <span className={styles.starLabel}>{star}★</span>
            <div className={styles.track}>
              <div className={styles.fill} style={{ width: `${pct}%` }} />
            </div>
            <span className={styles.count}>{count}</span>
          </div>
        );
      })}
    </div>
  );
}
