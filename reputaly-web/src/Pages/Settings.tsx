import { useState, useEffect } from 'react';
import { getTenant, updateTenant, getSettings, updateSettings } from '../api/tenantApi';
import type { Tenant, TenantSettings } from '../types/tenant';

export default function Settings() {
  // Estado para los datos del tenant
  const [, setTenant] = useState<Tenant | null>(null);
  const [settings, setSettings] = useState<TenantSettings | null>(null);
  
  // Estado para los campos del formulario
  const [tenantName, setTenantName] = useState('');
  const [notificationEmail, setNotificationEmail] = useState('');
  const [aiPersonality, setAiPersonality] = useState('');

  // Estado para feedback al usuario
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState('');

  // Cargar datos al montar el componente
  useEffect(() => {
    async function loadData() {
      try {
        const [tenantData, settingsData] = await Promise.all([
          getTenant(),
          getSettings(),
        ]);

        setTenant(tenantData);
        setSettings(settingsData);

        // Inicializar los campos del formulario con los datos actuales
        setTenantName(tenantData.name);
        console.log('notificationEmail recibido:', settingsData.notificationEmail);
        setNotificationEmail(settingsData.notificationEmail ?? '');
        setAiPersonality(settingsData.aiPersonality);
      } catch (error) {
        setMessage('Error al cargar los datos.');
      } finally {
        setLoading(false);
      }
    }

    loadData();
  }, []); // [] = solo se ejecuta una vez al entrar en la página

  async function handleSave() {
    setSaving(true);
    setMessage('');
    try {
      await updateTenant(tenantName);
      await updateSettings({
        ...settings!,
        notificationEmail: notificationEmail || null,
        aiPersonality,
      });
      setMessage('Cambios guardados correctamente.');
    } catch (error) {
      setMessage('Error al guardar los cambios.');
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <p>Cargando...</p>;

  return (
    <div style={{ maxWidth: '600px' }}>
      <h1 style={{ marginBottom: '32px' }}>Configuración</h1>

      <section style={{ marginBottom: '24px' }}>
        <label style={labelStyle}>Nombre del negocio</label>
        <input
          style={inputStyle}
          value={tenantName}
          onChange={(e) => setTenantName(e.target.value)}
        />
      </section>

      <section style={{ marginBottom: '24px' }}>
        <label style={labelStyle}>Email de notificaciones</label>
        <input
          style={inputStyle}
          type="email"
          value={notificationEmail}
          onChange={(e) => setNotificationEmail(e.target.value)}
        />
      </section>

      <section style={{ marginBottom: '32px' }}>
        <label style={labelStyle}>Personalidad de la IA</label>
        <textarea
          style={{ ...inputStyle, height: '100px', resize: 'vertical' }}
          value={aiPersonality}
          onChange={(e) => setAiPersonality(e.target.value)}
          placeholder="Ejemplo: Responde siempre en tono profesional y cercano, evita tecnicismos..."
        />
      </section>

      {message && (
        <p style={{ marginBottom: '16px', color: message.includes('Error') ? '#ef4444' : '#22c55e' }}>
          {message}
        </p>
      )}

      <button
        onClick={handleSave}
        disabled={saving}
        style={buttonStyle}
      >
        {saving ? 'Guardando...' : 'Guardar cambios'}
      </button>
    </div>
  );
}

const labelStyle: React.CSSProperties = {
  display: 'block',
  marginBottom: '8px',
  fontWeight: 600,
  color: '#374151',
};

const inputStyle: React.CSSProperties = {
  width: '100%',
  padding: '10px 12px',
  borderRadius: '6px',
  border: '1px solid #d1d5db',
  fontSize: '14px',
  outline: 'none',
};

const buttonStyle: React.CSSProperties = {
  backgroundColor: '#3b82f6',
  color: '#ffffff',
  padding: '10px 24px',
  borderRadius: '6px',
  border: 'none',
  fontWeight: 600,
  cursor: 'pointer',
  fontSize: '14px',
};