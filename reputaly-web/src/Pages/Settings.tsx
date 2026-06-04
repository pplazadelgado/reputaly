import { useState, useEffect } from 'react';
import { getTenant, updateTenant, getSettings, updateSettings } from '../api/tenantApi';
import type { Tenant, TenantSettings } from '../types/tenant';
import TopBar from '../components/layout/TopBar';
import { Button, Card, Field, Input, Textarea, ChipInput, useToast } from '../components/ui';
import styles from './Settings.module.css';

type Tab = 'general' | 'ia' | 'notificaciones' | 'google';

const TABS: { id: Tab; label: string }[] = [
  { id: 'general', label: 'General' },
  { id: 'ia', label: 'IA y respuestas' },
  { id: 'notificaciones', label: 'Notificaciones' },
  { id: 'google', label: 'Conexión Google' },
];

type Tone = 'formal' | 'cercano' | 'neutro';

const TONES: { id: Tone; emoji: string; label: string }[] = [
  { id: 'formal', emoji: '🧑‍💼', label: 'Formal' },
  { id: 'cercano', emoji: '😊', label: 'Cercano' },
  { id: 'neutro', emoji: '⚖️', label: 'Neutro' },
];

export default function Settings() {
  const { addToast } = useToast();

  const [, setTenant] = useState<Tenant | null>(null);
  const [settings, setSettings] = useState<TenantSettings | null>(null);

  const [tenantName, setTenantName] = useState('');
  const [notificationEmail, setNotificationEmail] = useState('');
  const [aiPersonality, setAiPersonality] = useState('');
  const [autoReplyMinRating, setAutoReplyMinRating] = useState(4);
  const [escalateOnKeywords, setEscalateOnKeywords] = useState<string[]>([]);

  const [activeTab, setActiveTab] = useState<Tab>('general');
  const [tone, setTone] = useState<Tone>('neutro');

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    async function loadData() {
      try {
        const [tenantData, settingsData] = await Promise.all([getTenant(), getSettings()]);
        setTenant(tenantData);
        setSettings(settingsData);
        setTenantName(tenantData.name);
        setNotificationEmail(settingsData.notificationEmail ?? '');
        setAiPersonality(settingsData.aiConfig?.default?.instructions ?? '');
        setAutoReplyMinRating(settingsData.autoReplyMinRating ?? 4);
        setEscalateOnKeywords(settingsData.escalateOnKeywords ?? []);
        console.log('keywords cargadas:', settingsData.escalateOnKeywords);
      } catch {
        addToast('Error al cargar los datos de configuración.', 'error');
      } finally {
        setLoading(false);
      }
    }
    loadData();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  async function handleSave() {
    setSaving(true);
    try {
      await updateTenant(tenantName);
      await updateSettings({
      ...settings!,
      notificationEmail: notificationEmail || null,
      autoReplyMinRating,
      escalateOnKeywords,
      aiConfig: {
        ...settings!.aiConfig,
        default: {
          ...settings!.aiConfig.default,
          instructions: aiPersonality,
        },
  },
});
      addToast('Cambios guardados correctamente.', 'success');
    } catch {
      addToast('Error al guardar los cambios. Inténtalo de nuevo.', 'error');
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <>
        <TopBar title="Configuración" />
        <div className={styles.content}>
          <p style={{ color: 'var(--slate-500)', fontSize: 14 }}>Cargando configuración…</p>
        </div>
      </>
    );
  }

  return (
    <>
      <TopBar title="Configuración" subtitle="Personaliza el comportamiento de Reputaly para tu negocio." />

      <div className={styles.content}>
        <div className={styles.tabs}>
          {TABS.map((t) => (
            <button
              key={t.id}
              type="button"
              className={[styles.tab, activeTab === t.id ? styles.active : ''].filter(Boolean).join(' ')}
              onClick={() => setActiveTab(t.id)}
            >
              {t.label}
            </button>
          ))}
        </div>

        {activeTab === 'general' && (
          <Card>
            <div className={styles.tabContent}>
              <div className={styles.fieldGrid}>
                <Field label="Nombre del negocio">
                  <Input
                    value={tenantName}
                    onChange={(e) => setTenantName(e.target.value)}
                    placeholder="Clínica Dental Ramírez"
                  />
                </Field>
              </div>
              <div className={styles.actions}>
                <Button variant="blue" loading={saving} onClick={handleSave}>
                  Guardar cambios
                </Button>
              </div>
            </div>
          </Card>
        )}

        {activeTab === 'ia' && (
          <Card>
            <div className={styles.tabContent}>
              <Field label="Tono de respuesta">
                <div className={styles.toneGroup} role="group" aria-label="Tono de respuesta">
                  {TONES.map((t) => (
                    <button
                      key={t.id}
                      type="button"
                      className={[styles.toneOption, tone === t.id ? styles.toneActive : ''].filter(Boolean).join(' ')}
                      onClick={() => setTone(t.id)}
                      aria-pressed={tone === t.id}
                    >
                      <span className={styles.toneEmoji} aria-hidden="true">{t.emoji}</span>
                      <span className={styles.toneName}>{t.label}</span>
                    </button>
                  ))}
                </div>
              </Field>

              <Field
                label="Umbral mínimo para auto-respuesta"
                hint={`Auto-responde reseñas de ${autoReplyMinRating} ★ o más.`}
              >
                <div className={styles.sliderWrapper}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)' }}>
                    <input
                      type="range"
                      className={styles.slider}
                      min={1}
                      max={5}
                      step={1}
                      value={autoReplyMinRating}
                      onChange={(e) => setAutoReplyMinRating(Number(e.target.value))}
                      aria-label="Umbral mínimo de valoración"
                    />
                    <span className={styles.sliderValue}>{autoReplyMinRating} ★</span>
                  </div>
                  <div className={styles.sliderLabels}>
                    <span>1 ★</span>
                    <span>5 ★</span>
                  </div>
                </div>
              </Field>

              <Field
                label="Palabras clave para escalar"
                hint="Escribe una palabra y pulsa Enter. Si la reseña las contiene, se escala al equipo."
              >
                <ChipInput
                  values={escalateOnKeywords}
                  onChange={setEscalateOnKeywords}
                  placeholder="Añade palabras clave…"
                />
              </Field>

              <Field
                label="Personalidad de la IA"
                hint="Describe cómo debe comportarse la IA al redactar respuestas."
              >
                <Textarea
                  value={aiPersonality}
                  onChange={(e) => setAiPersonality(e.target.value)}
                  placeholder="Ejemplo: Responde siempre en tono profesional y cercano, evita tecnicismos y agradece siempre la opinión..."
                  maxLength={500}
                />
              </Field>

              <div className={styles.actions}>
                <Button variant="blue" loading={saving} onClick={handleSave}>
                  Guardar cambios
                </Button>
              </div>
            </div>
          </Card>
        )}

        {activeTab === 'notificaciones' && (
          <Card>
            <div className={styles.tabContent}>
              <div className={styles.fieldGrid}>
                <Field
                  label="Email de notificaciones"
                  hint="Recibirás alertas sobre reseñas escaladas y eventos importantes."
                >
                  <Input
                    type="email"
                    value={notificationEmail}
                    onChange={(e) => setNotificationEmail(e.target.value)}
                    placeholder="correo@tuempresa.es"
                  />
                </Field>
              </div>
              <div className={styles.actions}>
                <Button variant="blue" loading={saving} onClick={handleSave}>
                  Guardar cambios
                </Button>
              </div>
            </div>
          </Card>
        )}

        {activeTab === 'google' && (
          <Card>
            <div className={styles.tabContent}>
              <p className={styles.comingSoonNote}>
                La conexión con Google Business Profile se configura durante el onboarding. Contacta con soporte si necesitas reconectar tu cuenta.
              </p>
            </div>
          </Card>
        )}
      </div>
    </>
  );
}
