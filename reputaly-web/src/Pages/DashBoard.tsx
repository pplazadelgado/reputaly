import { useState, useEffect, useMemo } from 'react';
import TopBar from '../components/layout/TopBar';
import {
  KpiCard,
  Card,
  LineChart,
  StarBars,
  Donut,
  Select,
  useToast,
} from '../components/ui';
import { getAnalytics } from '../api/analyticsApi';
import { getLocations } from '../api/tenantApi';
import type { Location } from '../api/tenantApi';
import type { Analytics } from '../types/analytics';
import styles from './DashBoard.module.css';

type RangePreset = '7d' | '30d' | '90d' | 'ytd';

const RANGE_OPTIONS = [
  { value: '7d', label: 'Últimos 7 días' },
  { value: '30d', label: 'Últimos 30 días' },
  { value: '90d', label: 'Últimos 90 días' },
  { value: 'ytd', label: 'Este año' },
];

function computeRange(preset: RangePreset): { from: string; to: string } {
  const now = new Date();
  const to = now.toISOString();
  const from = new Date(now);
  switch (preset) {
    case '7d':
      from.setDate(now.getDate() - 7);
      break;
    case '30d':
      from.setDate(now.getDate() - 30);
      break;
    case '90d':
      from.setDate(now.getDate() - 90);
      break;
    case 'ytd':
      from.setMonth(0, 1);
      from.setHours(0, 0, 0, 0);
      break;
  }
  return { from: from.toISOString(), to };
}

function formatHours(h: number | null): string {
  if (h === null) return '—';
  if (h < 1) return `${Math.round(h * 60)} min`;
  if (h < 24) return `${h.toFixed(1)} h`;
  return `${(h / 24).toFixed(1)} d`;
}

export default function Dashboard() {
  const { addToast } = useToast();

  const [range, setRange] = useState<RangePreset>('30d');
  const [locationId, setLocationId] = useState<string>('');
  const [locations, setLocations] = useState<Location[]>([]);

  const [analytics, setAnalytics] = useState<Analytics | null>(null);
  const [loading, setLoading] = useState(true);

  // Cargar ubicaciones una sola vez
  useEffect(() => {
    getLocations()
      .then(setLocations)
      .catch(() => {/* silencioso: si falla, el select queda solo con "Todas" */});
  }, []);

  // Recargar analytics cuando cambien filtros
  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    const { from, to } = computeRange(range);
    getAnalytics({
      from,
      to,
      locationId: locationId || undefined,
    })
      .then((data) => {
        if (!cancelled) setAnalytics(data);
      })
      .catch(() => {
        if (!cancelled) addToast('Error al cargar las métricas.', 'error');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [range, locationId]); // eslint-disable-line react-hooks/exhaustive-deps

  const locationOptions = useMemo(
    () => [
      { value: '', label: 'Todas las ubicaciones' },
      ...locations.map((l) => ({ value: l.id, label: l.name })),
    ],
    [locations],
  );

  const subtitle =
    RANGE_OPTIONS.find((r) => r.value === range)?.label ?? 'Resumen de actividad';

  const autoReplyPct =
    analytics && analytics.totalReviews > 0
      ? Math.round((analytics.statusBreakdown.autoReplied / analytics.totalReviews) * 100)
      : 0;

  const lineData =
    analytics?.ratingEvolution.map((p) => ({ label: p.label, value: p.value })) ?? [];

  const starDist =
    (analytics?.starDistribution ?? [0, 0, 0, 0, 0]) as [number, number, number, number, number];

  const donutSegments = analytics
    ? [
        { label: 'Auto-respondida', value: analytics.statusBreakdown.autoReplied, color: '#16A34A' },
        { label: 'Manual', value: analytics.statusBreakdown.replied, color: '#2563EB' },
        { label: 'Escalada', value: analytics.statusBreakdown.escalated, color: '#F59E0B' },
        { label: 'Pendiente', value: analytics.statusBreakdown.pending, color: '#94A3B8' },
      ].filter((s) => s.value > 0)
    : [];

  return (
    <>
      <TopBar title="Panel de control" subtitle={subtitle} />

      <div className={styles.content}>
        <div className={styles.filters}>
          <Select
            options={locationOptions}
            value={locationId}
            onChange={setLocationId}
          />
          <Select
            options={RANGE_OPTIONS}
            value={range}
            onChange={(v) => setRange(v as RangePreset)}
          />
        </div>

        {loading && !analytics ? (
          <p className={styles.loading}>Cargando métricas…</p>
        ) : !analytics || analytics.totalReviews === 0 ? (
          <Card>
            <p className={styles.empty}>
              No hay reseñas en el periodo seleccionado.
            </p>
          </Card>
        ) : (
          <>
            <div className={styles.kpiRow}>
              <KpiCard
                label="Valoración media"
                value={`${analytics.averageRating.toFixed(1)} ★`}
                subtitle={subtitle}
              />
              <KpiCard
                label="Total reseñas"
                value={analytics.totalReviews.toString()}
                subtitle={subtitle}
              />
              <KpiCard
                label="Auto-respondidas"
                value={`${autoReplyPct}%`}
                subtitle={`${analytics.statusBreakdown.autoReplied} de ${analytics.totalReviews}`}
              />
              <KpiCard
                label="Tiempo medio resp."
                value={formatHours(analytics.averageResponseTimeHours)}
                subtitle={subtitle}
              />
            </div>

            <div className={styles.chartsRow}>
              <Card>
                <LineChart
                  data={lineData}
                  title="Evolución de la valoración"
                  height={200}
                />
              </Card>

              <div className={styles.chartsInner}>
                <Card>
                  <p className={styles.sectionTitle}>Distribución por estrellas</p>
                  <StarBars distribution={starDist} />
                </Card>
                <Card>
                  <p className={styles.sectionTitle}>Tipo de respuesta</p>
                  {donutSegments.length > 0 ? (
                    <Donut segments={donutSegments} size={120} thickness={18} />
                  ) : (
                    <p className={styles.emptyMini}>Sin datos</p>
                  )}
                </Card>
              </div>
            </div>

            {analytics.topTopics.length > 0 && (
              <Card>
                <p className={styles.sectionTitle}>Temas más mencionados</p>
                <div className={styles.topicsList}>
                  {analytics.topTopics.map((t) => (
                    <span key={t.topic} className={styles.topicChip}>
                      {t.topic}
                      <span className={styles.topicCount}>{t.count}</span>
                    </span>
                  ))}
                </div>
              </Card>
            )}
          </>
        )}
      </div>
    </>
  );
}