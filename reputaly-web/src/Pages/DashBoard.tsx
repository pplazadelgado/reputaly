import TopBar from '../components/layout/TopBar';
import {
  KpiCard,
  Card,
  Avatar,
  Stars,
  StatusBadge,
  LineChart,
  StarBars,
  Donut,
} from '../components/ui';
import type { ReviewStatus, DataPoint } from '../components/ui';
import styles from './DashBoard.module.css';

// Datos de ejemplo — reemplazar con llamadas a la API cuando esté disponible
interface Review {
  id: string;
  author: string;
  rating: number;
  date: string;
  excerpt: string;
  status: ReviewStatus;
}

const SAMPLE_REVIEWS: Review[] = [
  {
    id: '1',
    author: 'María García López',
    rating: 5,
    date: '30 may',
    excerpt:
      'Excelente atención, el Dr. Ramírez fue muy profesional y nos explicó todo detalladamente. Completamente recomendable.',
    status: 'auto',
  },
  {
    id: '2',
    author: 'Carlos Ruiz Fernández',
    rating: 4,
    date: '29 may',
    excerpt:
      'Muy buena clínica, instalaciones modernas y personal amable. El tiempo de espera fue algo largo pero mereció la pena.',
    status: 'replied',
  },
  {
    id: '3',
    author: 'Laura Martínez Sánchez',
    rating: 2,
    date: '28 may',
    excerpt:
      'Tuve que esperar más de una hora para una cita con hora. La atención fue correcta pero la organización deja mucho que desear.',
    status: 'escalated',
  },
  {
    id: '4',
    author: 'Javier López Hernández',
    rating: 5,
    date: '27 may',
    excerpt:
      'Fui por primera vez y quedé encantado. La higienista fue muy meticulosa y la clínica estaba impecable.',
    status: 'pending',
  },
  {
    id: '5',
    author: 'Ana Gómez Torres',
    rating: 3,
    date: '26 may',
    excerpt:
      'Atención correcta pero nada especial. El precio me pareció algo elevado para lo que ofrecen.',
    status: 'auto',
  },
];

function buildLineData(): DataPoint[] {
  const values = [4,3,6,5,8,7,9,6,10,8,11,9,7,12,10,14,11,9,13,15,12,10,14,11,13,16,14,12,15,13];
  const labels = ['1 may','2','3','4','5','6','7','8','9','10','11','12','13','14','15','16','17','18','19','20','21','22','23','24','25','26','27','28','29','30'];
  return values.map((value, i) => ({ label: labels[i], value }));
}

const LINE_DATA = buildLineData();
const STAR_DIST: [number, number, number, number, number] = [142, 68, 21, 9, 7];

const DONUT_SEGMENTS = [
  { label: 'Auto-respondida', value: 189, color: '#16A34A' },
  { label: 'Manual', value: 34, color: '#2563EB' },
  { label: 'Pendiente', value: 8, color: '#94A3B8' },
  { label: 'Error', value: 16, color: '#DC2626' },
];

export default function Dashboard() {
  return (
    <>
      <TopBar
        title="Panel de control"
        subtitle="Resumen de actividad — últimos 30 días"
        notifications
      />

      <div className={styles.content}>
        <div className={styles.kpiRow}>
          <KpiCard label="Reseñas este mes" value="247" trend={12.4} />
          <KpiCard label="Valoración media" value="4,6 ★" trend={0.2} />
          <KpiCard label="Auto-respondidas" value="189" trend={18.1} />
          <KpiCard label="Pendientes" value="8" trend={-2.5} />
        </div>

        <div className={styles.chartsRow}>
          <Card>
            <LineChart data={LINE_DATA} title="Reseñas recibidas" height={180} />
          </Card>

          <div className={styles.chartsInner}>
            <Card>
              <p className={styles.sectionTitle} style={{ marginBottom: 'var(--space-4)' }}>
                Distribución por estrellas
              </p>
              <StarBars distribution={STAR_DIST} />
            </Card>
            <Card>
              <p className={styles.sectionTitle} style={{ marginBottom: 'var(--space-4)' }}>
                Tipo de respuesta
              </p>
              <Donut segments={DONUT_SEGMENTS} size={100} thickness={16} />
            </Card>
          </div>
        </div>

        <Card>
          <div className={styles.reviewsHeader}>
            <h2 className={styles.sectionTitle}>Reseñas recientes</h2>
          </div>
          <div className={styles.reviewList}>
            {SAMPLE_REVIEWS.map((r) => (
              <div key={r.id} className={styles.reviewRow}>
                <Avatar name={r.author} size={40} />
                <div className={styles.reviewMeta}>
                  <span className={styles.reviewAuthor}>{r.author}</span>
                  <div className={styles.reviewStarsDate}>
                    <Stars value={r.rating} size={13} />
                    <span className={styles.reviewDate}>{r.date}</span>
                  </div>
                  <p className={styles.reviewExcerpt}>{r.excerpt}</p>
                </div>
                <StatusBadge status={r.status} />
              </div>
            ))}
          </div>
        </Card>
      </div>
    </>
  );
}
