import {useState, useEffect} from 'react';
import TopBar from '../components/layout/TopBar';
import{
    Card,
    Button,
    Stars,
    StatusBadge,
    Avatar,
    Select,
    Drawer
} from '../components/ui';
import type {ReviewStatus as BadgeStatus} from "../components/ui";
import {useToast} from '../components/ui';
import apiClient from '../api/apiClient';
import type { Review, ReviewsPage } from '../types/review';
import styles from './Reviews.module.css';
import { useSearchParams } from 'react-router-dom';

// -------------------------------------------------------
// Helpers
// -------------------------------------------------------

// El StatusBadge del UI kit espera "autoÇ", pero el backend devuelve "auto_replied"
// Esta funcion convierte entre ambos formatos
function toBadgeStatus(status: string): BadgeStatus {
    if(status === 'auto_replied') return 'auto';
    return status as BadgeStatus;
}

// Convierte el sentimentScore numerico en un indicador visual.
function sentimentLabel(score: number | null): { text: string; className: string } {
  if (score === null) return { text: '—', className: '' };
  if (score > 0.3) return { text: 'Positivo', className: styles.sentimentPositive };
  if (score >= -0.3) return { text: 'Neutro', className: styles.sentimentNeutral };
  return { text: 'Negativo', className: styles.sentimentNegative };
}

// Formatea una fecha ISO en un formato legible.
function formatDate(iso:string): string {
    return new Date(iso).toLocaleDateString('es-ES', {
        day : 'numeric',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}
    
// Opciones para los filtros Select
const STATUS_OPTIONS = [
    { value:'', label: 'Todos los estados' },
    {value:'pending', label: 'Pendientes'},
    {value:'auto_replied', label: 'Auto-respondidas '},
    {value:'escalated', label: 'Escaladas'},
    {value:'replied', label: 'Respondidas'},
];

const RATING_OPTIONS = [
    { value: '', label: 'Todas las valoraciones' },
    { value: '5', label: '5 estrellas' },
    { value: '4', label: '4 estrellas' },
    { value: '3', label: '3 estrellas' },
    { value: '2', label: '2 estrellas' },
    { value: '1', label: '1 estrella' },
];


// -------------------------------------------------------
// Componente principal
// -------------------------------------------------------
export default function Reviews() {
    // ---Estado---
    // En React, cada useState crea una variable reactiva: cuando cambia,
    // el componente se vuelve a renderizar para reflejar el nuevo estado.
    const [reviews, setReviews] = useState<Review[]>([]);
    const [total, setTotal] = useState(0);
    const [page, setPage] = useState(1);
    const [loading, setLoading] = useState(true);
    const [statusFilter, setStatusFilter] = useState('');
    const [ratingFilter, setRatingFilter] = useState('');

    // Estado del Drawer de detalle
    const [selectedReview, setSelectedReview] = useState<Review | null>(null);
    const [replyText, setReplyText] = useState('');
    const [sending, setSending] = useState(false);
    const [searchParams, setSearchParams] = useSearchParams();

    const { addToast } = useToast();
    const pageSize = 20;

    // -- carga de datos ---
    // useEffect es un hook que ejecuta su función cada vez que cambian las dependencias (en este caso, page, statusFilter o ratingFilter).
    // Esto nos permite recargar la lista de reseñas cada vez que el usuario cambia de página o aplica un filtro.
    useEffect(() => {
        loadReviews();
    }, [page, statusFilter, ratingFilter]);

    //Si llegamos con ?review={id} abrimos el drawer
    //de esa reseña concreta, pidiendola al backend por su ID
    useEffect(() => {
        const reviewId = searchParams.get('review');
        if (!reviewId) return;

        async function openReviewFromUrl(id: string) {
            try {
                const { data } = await apiClient.get<Review>(`/reviews/${id}`);
                setSelectedReview(data);
                setReplyText('');
            } catch (err) {
                addToast('No se pudo abrir la reseña indicada.', 'error');
            } finally {
                setSearchParams({}, { replace: true });
            }
        }

        openReviewFromUrl(reviewId);
    }, [searchParams]); // eslint-disable-line react-hooks/exhaustive-deps

    async function loadReviews() {
        setLoading(true);
        try{
            // Construimos las query params dinamicamente
            // Solo enviamos un parametro si tiene valor, par no mandar ?status=&rating=vacio
            const params: Record<string, string | number> = { page, pageSize };
            if(statusFilter) params.status = statusFilter;
            if(ratingFilter) params.rating = Number(ratingFilter);

            const {data} = await apiClient.get<ReviewsPage>('/reviews', { params });
            setReviews(data.items);
            setTotal(data.total);
        }catch{
            addToast('Error al cargar las reseñas', 'error');
        }finally{
            setLoading(false);
        }
    }

    // --- Acciones ---
    async function handleApprove(review: Review) {
        setSending(true);
        try{
            await apiClient.post(`/reviews/${review.id}/approve`);
            addToast('Respuesta aprovada correctamente', 'success');
            setSelectedReview(null);
            loadReviews(); // Recarga la lista para reflejar el cambio
        } catch{
            addToast('Error al aprobar la respuesta', 'error');
        } finally{
            setSending(false);
        }
    }

    async function handleReply(review: Review) {
        if(!replyText.trim()) return; // No enviar respuestas vacías
        setSending(true);
        try{
            await apiClient.post(`/reviews/${review.id}/reply`, { text: replyText });
            addToast('Respuesta publicada correctamente', 'success');
            setSelectedReview(null);
            setReplyText('');
            loadReviews(); // Recarga la lista para reflejar el cambio
        }catch{
            addToast ('Error al publicar la respuesta', 'error');
        }finally{
            setSending(false);
        }
    }

    // Cuando el usuario ccambia un filtro, reseteamos la pagina 1
    function handleStatusChange(value: string) {
        setStatusFilter(value);
        setPage(1);
    }

    function handleRatingChange(value: string) {
        setRatingFilter(value);
        setPage(1);
    }

    const totalPages = Math.ceil(total / pageSize);

    // -------------------------------------------------------
  // Render
  // -------------------------------------------------------
   return (
    <>
      <TopBar
        title="Reseñas"
        subtitle={`${total} reseñas en total`}
      />

      <div className={styles.content}>
        {/* ---- Filtros ---- */}
        <div className={styles.filters}>
          <Select
            options={STATUS_OPTIONS}
            value={statusFilter}
            onChange={handleStatusChange}
          />
          <Select
            options={RATING_OPTIONS}
            value={ratingFilter}
            onChange={handleRatingChange}
          />
        </div>

        {/* ---- Lista ---- */}
        {loading ? (
          <Card>
            <p className={styles.loadingText}>Cargando reseñas...</p>
          </Card>
        ) : reviews.length === 0 ? (
          <Card>
            <p className={styles.emptyText}>No hay reseñas con estos filtros.</p>
          </Card>
        ) : (
          <div className={styles.list}>
            {/* .map() transforma cada review del array en un bloque JSX.
                La prop key={review.id} es obligatoria: React la usa internamente
                para saber qué elementos añadir/quitar/mover cuando cambia la lista. */}
            {reviews.map((review) => {
              const sentiment = sentimentLabel(review.sentimentScore);
              return (
                <Card key={review.id}>
                  <button
                    className={styles.reviewRow}
                    onClick={() => {
                      setSelectedReview(review);
                      setReplyText('');
                    }}
                  >
                    <Avatar name={review.authorName} size={40} />

                    <div className={styles.reviewBody}>
                      <div className={styles.reviewHeader}>
                        <span className={styles.authorName}>{review.authorName}</span>
                        <Stars value={review.rating} size={16} />
                        <StatusBadge status={toBadgeStatus(review.status)} />
                        {sentiment.className && (
                          <span className={`${styles.sentimentChip} ${sentiment.className}`}>
                            {sentiment.text}
                          </span>
                        )}
                      </div>

                      <p className={styles.reviewContent}>
                        {review.content || '(Sin texto, solo valoración)'}
                      </p>

                      <div className={styles.reviewMeta}>
                        <span>{formatDate(review.publishedAt)}</span>
                        {review.detectedLanguage && (
                          <span className={styles.langBadge}>
                            {review.detectedLanguage.toUpperCase()}
                          </span>
                        )}
                        {review.detectedTopics?.map((topic) => (
                          <span key={topic} className={styles.topicChip}>
                            {topic}
                          </span>
                        ))}
                      </div>
                    </div>
                  </button>
                </Card>
              );
            })}
          </div>
        )}

        {/* ---- Paginación ---- */}
        {totalPages > 1 && (
          <div className={styles.pagination}>
            <Button
              variant="ghost"
              size="sm"
              disabled={page <= 1}
              onClick={() => setPage(page - 1)}
            >
              ← Anterior
            </Button>
            <span className={styles.pageInfo}>
              Página {page} de {totalPages}
            </span>
            <Button
              variant="ghost"
              size="sm"
              disabled={page >= totalPages}
              onClick={() => setPage(page + 1)}
            >
              Siguiente →
            </Button>
          </div>
        )}

        {/* ---- Drawer de detalle ---- */}
        {/* El Drawer se abre cuando selectedReview no es null.
            Al cerrar, ponemos selectedReview a null y desaparece. */}
        <Drawer
          open={selectedReview !== null}
          onClose={() => setSelectedReview(null)}
          title="Detalle de reseña"
        >
          {selectedReview && (
            <div className={styles.drawerContent}>
              {/* Cabecera */}
              <div className={styles.drawerHeader}>
                <Avatar name={selectedReview.authorName} size={48} />
                <div>
                  <h3 className={styles.drawerAuthor}>{selectedReview.authorName}</h3>
                  <div className={styles.drawerRating}>
                    <Stars value={selectedReview.rating} size={18} />
                    <StatusBadge status={toBadgeStatus(selectedReview.status)} />
                  </div>
                  <span className={styles.drawerDate}>
                    {formatDate(selectedReview.publishedAt)}
                  </span>
                </div>
              </div>

              {/* Reseña */}
              <div className={styles.section}>
                <h4 className={styles.sectionTitle}>Reseña</h4>
                <p className={styles.reviewFullText}>
                  {selectedReview.content || '(Sin texto, solo valoración con estrellas)'}
                </p>
              </div>

              {/* Análisis IA */}
              {selectedReview.aiDecision && (
                <div className={styles.section}>
                  <h4 className={styles.sectionTitle}>Análisis IA</h4>
                  <div className={styles.analysisGrid}>
                    <span className={styles.analysisLabel}>Decisión</span>
                    <span>{selectedReview.aiDecision === 'auto_reply' ? 'Auto-respuesta' : 'Escalada'}</span>

                    <span className={styles.analysisLabel}>Motivo</span>
                    <span>{selectedReview.aiDecisionReason}</span>

                    {selectedReview.sentimentScore !== null && (
                      <>
                        <span className={styles.analysisLabel}>Sentimiento</span>
                        <span className={sentimentLabel(selectedReview.sentimentScore).className}>
                          {sentimentLabel(selectedReview.sentimentScore).text}
                          {' '}({selectedReview.sentimentScore.toFixed(2)})
                        </span>
                      </>
                    )}

                    {selectedReview.detectedLanguage && (
                      <>
                        <span className={styles.analysisLabel}>Idioma</span>
                        <span>{selectedReview.detectedLanguage.toUpperCase()}</span>
                      </>
                    )}
                  </div>

                  {selectedReview.detectedTopics && selectedReview.detectedTopics.length > 0 && (
                    <div className={styles.topicsRow}>
                      <span className={styles.analysisLabel}>Temas</span>
                      <div className={styles.topicChips}>
                        {selectedReview.detectedTopics.map((t) => (
                          <span key={t} className={styles.topicChip}>{t}</span>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              )}

              {/* Respuesta sugerida por IA */}
              {selectedReview.aiSuggestedReply && (
                <div className={styles.section}>
                  <h4 className={styles.sectionTitle}>Respuesta sugerida por IA</h4>
                  <p className={styles.aiReply}>{selectedReview.aiSuggestedReply}</p>

                  {/* Solo mostramos el botón de aprobar si la reseña no está ya respondida */}
                  {selectedReview.status !== 'replied' && selectedReview.status !== 'auto_replied' && (
                    <Button
                      variant="blue"
                      size="sm"
                      loading={sending}
                      onClick={() => handleApprove(selectedReview)}
                    >
                      Aprobar respuesta IA
                    </Button>
                  )}
                </div>
              )}

              {/* Respuesta final (si ya existe) */}
              {selectedReview.finalReply && (
                <div className={styles.section}>
                  <h4 className={styles.sectionTitle}>Respuesta publicada</h4>
                  <p className={styles.finalReply}>{selectedReview.finalReply}</p>
                  {selectedReview.repliedAt && (
                    <span className={styles.repliedDate}>
                      Respondida el {formatDate(selectedReview.repliedAt)}
                      {selectedReview.autoReplied && ' (automática)'}
                    </span>
                  )}
                </div>
              )}

              {/* Respuesta manual (solo si no está respondida) */}
              {selectedReview.status !== 'replied' && selectedReview.status !== 'auto_replied' && (
                <div className={styles.section}>
                  <h4 className={styles.sectionTitle}>Responder manualmente</h4>
                  <textarea
                    className={styles.replyTextarea}
                    value={replyText}
                    onChange={(e) => setReplyText(e.target.value)}
                    placeholder="Escribe tu respuesta..."
                    rows={4}
                  />
                  <Button
                    variant="primary"
                    size="sm"
                    loading={sending}
                    disabled={!replyText.trim()}
                    onClick={() => handleReply(selectedReview)}
                  >
                    Publicar respuesta
                  </Button>
                </div>
              )}
            </div>
          )}
        </Drawer>
      </div>
    </>
  );

}