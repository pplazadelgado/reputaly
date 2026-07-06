import {useState, useEffect} from 'react';
import { createCheckoutSession, createBillingPortalSession } from '../api/billingApi';
import TopBar from '../components/layout/TopBar';
import { Card, ProgressBar, Button, useToast } from '../components/ui';
import styles from './Billing.module.css';
import {useBilling} from '../context/BillingContext';
import { useSearchParams } from 'react-router-dom';


// Etiqueta legible para cada plan (el backend devuelve "free", "starter" o "pro").
const PLAN_LABELS: Record<string, string> = {
    free: 'Free',
    starter: 'Starter',
    pro: 'Pro'
};

// Definición de los planes que se muestran en las cards.
// rank establece el orden: free(0) < starter(1) < pro(2).
// Los precios y features son informativos (para la UI); la fuente
// de verdad de los límites es el backend.
interface PlanCard{
    key: string; // "free", "starter" o "pro"
    name: string; // "Free", "Starter" o "Pro"
    price: string; 
    rank: number;
    features: string[];
}

const PLANS_CARDS: PlanCard[] = [
    {
        key: 'free',
        name: 'Free',
        price: '0 €/mes',
        rank: 0,
        features: ['1 ubicacion','Sin respuestas IA','Panel basico']
    },
    {
        key: 'starter',
        name: 'Starter',
        price: '29 €/mes',
        rank: 1,
        features: ['1 ubicación', '100 respuestas IA / mes', 'Auto-respuesta']
    },
    {
        key: 'pro',
        name: 'Pro',
        price: '79 €/mes',
        rank: 2,
        features: ['Ubicaiones ilimitadas','Ilimitadas respuestas IA', 'Auto-respuesta']
    },
];

// Rango numérico del plan actual, para comparar con cada card.
const PLAN_RANK: Record<string, number> = { free: 0, starter: 1, pro: 2 };

export default function Billing() {
    const {addToast} = useToast();

    const {status, loading, refetch} = useBilling();
    const [searchParams, setSearchParams] = useSearchParams();
    const [processing, setProcessing] = useState(false); // Para bloquear botones mientras se redirige a Stripe  

    // Detecta la vuelta de Stripe (success_url / cancel_url) y reacciona
    useEffect(() =>{
      const checkout = searchParams.get('checkout');
      if(!checkout) return;

      if(checkout === 'success'){
        addToast('¡Suscripcion activada! Tu plna se actualizara en unos segundos', 'success');
        // El webhook de Stripe puede tardar 1-2s en actualizar el plan en el backend,
        // así que esperamos un poco antes de releer el estado.
        const timer = setTimeout(() =>{
          refetch();
        },2000);
        //Limpiamos el parametro de la URL para que no se repita al recarcar
        setSearchParams({},{replace:true});
        return ()=> clearTimeout(timer);
      }

      if(checkout=== 'cancel'){
        addToast('Has cancelado el proceso de pago','info');
        setSearchParams({},{replace:true});
      }
    },[searchParams, refetch, addToast,setSearchParams]);

    

    // El usuario quiere MEJORAR a un plan de pago desde Free → checkout de Stripe.
    async function handleUpgrade(planKey: string) {
        setProcessing(true);
        try{
            const url = await createCheckoutSession(planKey);
            window.location.href=url; // Redirige a Stripe
        }catch{
            addToast('No se pudo iniciar el pago. Inténtalo de nuevo.', 'error');
            setProcessing(false); // solo reactivamos si falló; si va bien, ya hemos salido
        }
    }

    // El usuario ya tiene plan de pago y quiere gestionarlo/cambiarlo → portal de Stripe.
    async function handleManage () {
        setProcessing(true);
        try{
            const url = await createBillingPortalSession();
            window.location.href = url;
        }
        catch {
            addToast('No se pudo abrir el portal de gestión. Inténtalo de nuevo.', 'error');
            setProcessing(false);
    }
    }

   return (
    <>
      <TopBar title="Facturación" subtitle="Gestiona tu plan y consulta tu uso." />
      <div className={styles.content}>
        {loading && <p className={styles.loadingText}>Cargando…</p>}

        {!loading && status && (
          <>
            {/* Tarjeta de plan actual + uso */}
            <Card>
              <div className={styles.currentPlan}>
                <div className={styles.planHeader}>
                  <span className={styles.planLabel}>Tu plan actual</span>
                  <span className={styles.planBadge}>
                    {PLAN_LABELS[status.plan] ?? status.plan}
                  </span>
                </div>

                {/* Uso de respuestas IA este mes */}
                <div className={styles.usageBlock}>
                  <div className={styles.usageLabel}>Respuestas IA este mes</div>
                  {status.monthlyAiReplies === -1 ? (
                    <p className={styles.usageUnlimited}>
                      {status.aiRepliesUsed} usadas · Ilimitadas en tu plan
                    </p>
                  ) : (
                    <>
                      <ProgressBar
                        value={status.aiRepliesUsed}
                        max={status.monthlyAiReplies}
                        color="navy"
                      />
                      <div className={styles.usageMeta}>
                        {status.aiRepliesUsed} / {status.monthlyAiReplies} este mes
                      </div>
                    </>
                  )}
                </div>

                {/* Fecha de renovación, solo si hay suscripción activa */}
                {status.currentPeriodEnd && (
                  <p className={styles.renewalInfo}>
                    {status.cancelAtPeriodEnd
                      ? `Tu suscripción se cancela el ${formatDate(status.currentPeriodEnd)}.`
                      : `Próxima renovación: ${formatDate(status.currentPeriodEnd)}.`}
                  </p>
                )}
              </div>
            </Card>

            {/* Cards de los tres planes */}
            <div className={styles.planGrid}>
              {PLANS_CARDS.map((p) => {
                const currentRank = PLAN_RANK[status.plan] ?? 0;
                const isCurrent = p.key === status.plan;
                const isUpgrade = p.rank > currentRank;
                const onFreePlan = status.plan === 'free';

                return (
                  <Card key={p.key}>
                    <div className={styles.planCardInner}>
                      <h3 className={styles.planName}>{p.name}</h3>
                      <p className={styles.planPrice}>{p.price}</p>

                      <ul className={styles.featureList}>
                        {p.features.map((f) => (
                          <li key={f} className={styles.featureItem}>{f}</li>
                        ))}
                      </ul>

                      <div className={styles.planAction}>
                        {isCurrent ? (
                          <Button variant="ghost" disabled>Plan actual</Button>
                        ) : p.key === 'free' ? (
                          // Nadie "compra" el plan Free; para bajar a Free se usa el portal.
                          <Button
                            variant="blue"
                            loading={processing}
                            onClick={handleManage}
                          >
                            Cambiar
                          </Button>
                        ) : onFreePlan && isUpgrade ? (
                          <Button
                            variant="blue"
                            loading={processing}
                            onClick={() => handleUpgrade(p.key)}
                          >
                            Mejorar
                          </Button>
                        ) : (
                          // Ya tiene plan de pago: cualquier cambio va por el portal.
                          <Button
                            variant="blue"
                            loading={processing}
                            onClick={handleManage}
                          >
                            Cambiar
                          </Button>
                        )}
                      </div>
                    </div>
                  </Card>
                );
              })}
            </div>

            {/* Botón global de gestión de suscripción, solo si tiene plan de pago */}
            {status.plan !== 'free' && (
              <div className={styles.manageRow}>
                <Button variant="ghost" loading={processing} onClick={handleManage}>
                  Gestionar suscripción
                </Button>
              </div>
            )}

          </>
        )}
      </div>
    </>
   );
}

// Convierte una fecha ISO a ("día/mes/año ")
function formatDate(iso:string): string {
    return new Date(iso).toLocaleDateString('es-ES', {
        day: 'numeric',
        month: 'long',
        year: 'numeric'
    });
}