import styles from './LineChart.module.css';

export interface DataPoint {
  label: string;
  value: number;
}

export interface LineChartProps {
  data: DataPoint[];
  title?: string;
  height?: number;
}

export function LineChart({ data, title, height = 160 }: LineChartProps) {
  if (data.length === 0) return null;

  const padT = 8;
  const padB = 28;
  const padL = 32;
  const padR = 8;
  const w = 600;
  const h = height;
  const innerW = w - padL - padR;
  const innerH = h - padT - padB;

  const maxVal = Math.max(...data.map((d) => d.value));
  const minVal = Math.min(...data.map((d) => d.value));
  const range = maxVal - minVal || 1;

  const xStep = innerW / (data.length - 1 || 1);

  const toX = (i: number) => padL + i * xStep;
  const toY = (v: number) => padT + innerH - ((v - minVal) / range) * innerH;

  const pathD = data
    .map((d, i) => `${i === 0 ? 'M' : 'L'} ${toX(i)} ${toY(d.value)}`)
    .join(' ');

  const areaD =
    `M ${toX(0)} ${toY(data[0].value)} ` +
    data.slice(1).map((d, i) => `L ${toX(i + 1)} ${toY(d.value)}`).join(' ') +
    ` L ${toX(data.length - 1)} ${padT + innerH} L ${toX(0)} ${padT + innerH} Z`;

  const gridCount = 4;
  const gridLines = Array.from({ length: gridCount + 1 }, (_, i) => {
    const v = minVal + (range * i) / gridCount;
    const y = toY(v);
    return { y, label: Math.round(v).toString() };
  });

  const xLabels = data
    .filter((_, i) => i % Math.ceil(data.length / 7) === 0 || i === data.length - 1)
    .map((d, _, arr) => {
      const realIndex = data.findIndex((dd) => dd === d);
      return { x: toX(realIndex), label: arr.indexOf(d) === arr.length - 1 ? d.label : d.label };
    });

  return (
    <div className={styles.root}>
      {title && (
        <div className={styles.header}>
          <span className={styles.chartTitle}>{title}</span>
        </div>
      )}
      <svg
        className={styles.svg}
        viewBox={`0 0 ${w} ${h}`}
        aria-label={title}
        role="img"
      >
        <defs>
          <linearGradient id="lineGradient" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor="#0B2545" />
            <stop offset="100%" stopColor="#0B2545" stopOpacity="0" />
          </linearGradient>
        </defs>

        {gridLines.map((g, i) => (
          <g key={i}>
            <line
              className={styles.gridLine}
              x1={padL}
              y1={g.y}
              x2={padL + innerW}
              y2={g.y}
            />
            <text className={styles.axisLabel} x={padL - 4} y={g.y + 4} textAnchor="end">
              {g.label}
            </text>
          </g>
        ))}

        {xLabels.map((xl, i) => (
          <text key={i} className={styles.axisLabel} x={xl.x} y={padT + innerH + 18} textAnchor="middle">
            {xl.label}
          </text>
        ))}

        <path className={styles.area} d={areaD} />
        <path className={styles.line} d={pathD} />
      </svg>
    </div>
  );
}
