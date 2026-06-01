import styles from './Stars.module.css';

export interface StarsProps {
  value: number;
  size?: number;
  color?: string;
  className?: string;
}

export function Stars({ value, size = 16, color = '#F59E0B', className }: StarsProps) {
  const stars = Array.from({ length: 5 }, (_, i) => {
    const fill = Math.min(1, Math.max(0, value - i));
    return fill;
  });

  return (
    <span
      className={[styles.stars, className ?? ''].filter(Boolean).join(' ')}
      aria-label={`${value.toFixed(1)} de 5 estrellas`}
      role="img"
    >
      {stars.map((fill, i) => (
        <StarSvg key={i} fill={fill} size={size} color={color} id={`star-${i}-${Math.random().toString(36).slice(2)}`} />
      ))}
    </span>
  );
}

function StarSvg({
  fill,
  size,
  color,
  id,
}: {
  fill: number;
  size: number;
  color: string;
  id: string;
}) {
  const gradientId = `grad-${id}`;

  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 20 20"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      aria-hidden="true"
    >
      <defs>
        <linearGradient id={gradientId}>
          <stop offset={`${fill * 100}%`} stopColor={color} />
          <stop offset={`${fill * 100}%`} stopColor="#E2E8F0" />
        </linearGradient>
      </defs>
      <path
        d="M10 1.5l2.39 5.26 5.44.44-4.04 3.84 1.17 5.46L10 13.77l-4.96 2.73 1.17-5.46L2.17 7.2l5.44-.44L10 1.5z"
        fill={`url(#${gradientId})`}
      />
    </svg>
  );
}
