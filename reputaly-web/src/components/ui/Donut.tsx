import styles from './Donut.module.css';

export interface DonutSegment {
  label: string;
  value: number;
  color: string;
}

export interface DonutProps {
  segments: DonutSegment[];
  size?: number;
  thickness?: number;
}

export function Donut({ segments, size = 120, thickness = 20 }: DonutProps) {
  const total = segments.reduce((s, seg) => s + seg.value, 0) || 1;
  const r = (size - thickness) / 2;
  const cx = size / 2;
  const cy = size / 2;
  const circumference = 2 * Math.PI * r;

  let offset = 0;
  const arcs = segments.map((seg) => {
    const pct = seg.value / total;
    const arc = {
      ...seg,
      pct,
      dasharray: `${pct * circumference} ${circumference}`,
      rotation: offset * 360 - 90,
    };
    offset += pct;
    return arc;
  });

  return (
    <div className={styles.root}>
      <svg
        className={styles.svg}
        width={size}
        height={size}
        viewBox={`0 0 ${size} ${size}`}
        aria-label="Distribución de respuestas"
        role="img"
      >
        {arcs.map((arc, i) => (
          <circle
            key={i}
            cx={cx}
            cy={cy}
            r={r}
            fill="none"
            stroke={arc.color}
            strokeWidth={thickness}
            strokeDasharray={arc.dasharray}
            transform={`rotate(${arc.rotation} ${cx} ${cy})`}
          />
        ))}
      </svg>
      <div className={styles.legend}>
        {arcs.map((arc, i) => (
          <div key={i} className={styles.legendItem}>
            <span className={styles.dot} style={{ backgroundColor: arc.color }} aria-hidden="true" />
            <span>{arc.label}</span>
            <span className={styles.legendPct}>{Math.round(arc.pct * 100)}%</span>
          </div>
        ))}
      </div>
    </div>
  );
}
