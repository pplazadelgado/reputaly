interface LogoProps {
  size?: number;
  variant?: 'light' | 'dark';
}

export function Logo({ size = 32, variant = 'light' }: LogoProps) {
  const textColor = variant === 'dark' ? '#7DA8FF' : '#2563EB';
  const bgColor = '#0B2545';

  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 32 32"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      aria-hidden="true"
    >
      <rect width="32" height="32" rx="7" fill={bgColor} />
      <text
        x="16"
        y="24"
        fontFamily="Manrope, Inter, sans-serif"
        fontSize="20"
        fontWeight="800"
        fill={textColor}
        textAnchor="middle"
      >
        R
      </text>
    </svg>
  );
}

interface WordmarkProps {
  size?: number;
  variant?: 'light' | 'dark';
}

export function Wordmark({ size = 32, variant = 'light' }: WordmarkProps) {
  const labelColor = variant === 'dark' ? '#F8FAFC' : '#0B2545';

  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
      <Logo size={size} variant={variant} />
      <span
        style={{
          fontFamily: 'var(--font-ui)',
          fontWeight: 800,
          fontSize: `${Math.round(size * 0.6)}px`,
          color: labelColor,
          letterSpacing: '-0.02em',
          lineHeight: 1,
        }}
      >
        Reputaly
      </span>
    </div>
  );
}
