import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, useNavigate } from 'react-router-dom';
import { ClerkProvider, useAuth } from '@clerk/clerk-react';
import { esES } from '@clerk/localizations';
import '@fontsource/manrope/400.css';
import '@fontsource/manrope/500.css';
import '@fontsource/manrope/600.css';
import '@fontsource/manrope/700.css';
import '@fontsource/manrope/800.css';
import './styles/tokens.css';
import './index.css';
import App from './App';
import { setupApiClient } from './api/apiClient';
import { ToastProvider } from './components/ui';

const publishableKey = import.meta.env.VITE_CLERK_PUBLISHABLE_KEY;

if (!publishableKey) {
  throw new Error('Falta VITE_CLERK_PUBLISHABLE_KEY en .env.local');
}

const clerkAppearance = {
  variables: {
    colorPrimary: '#0B2545',
    colorBackground: '#FFFFFF',
    colorText: '#0F172A',
    colorTextSecondary: '#334155',
    colorInputBackground: '#FFFFFF',
    colorInputText: '#0F172A',
    colorNeutral: '#64748B',
    colorDanger: '#DC2626',
    colorSuccess: '#16A34A',
    colorWarning: '#D97706',
    fontFamily: '"Manrope", "Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", system-ui, sans-serif',
    fontWeight: {
      normal: 400,
      medium: 500,
      semibold: 600,
      bold: 700,
    },
    borderRadius: '8px',
  },
  elements: {
    // Tarjeta contenedora: mismo radio y sombra que <Card />
    card: {
      borderRadius: '12px',
      boxShadow: '0 1px 2px rgba(15,23,42,0.04), 0 1px 3px rgba(15,23,42,0.06)',
      border: '1px solid #E2E8F0',
    },
    headerTitle: {
      fontSize: '20px',
      fontWeight: 700,
      color: '#0F172A',
    },
    headerSubtitle: {
      fontSize: '13px',
      color: '#64748B',
    },

    // Botones sociales (Google/Microsoft): igual que Button variant="secondary"
    socialButtonsBlockButton: {
      height: '40px',
      borderRadius: '8px',
      borderColor: '#E2E8F0',
      backgroundColor: '#FFFFFF',
      color: '#0B2545',
      fontWeight: 600,
      '&:hover': { backgroundColor: '#F8FAFC' },
    },
    socialButtonsBlockButtonText: {
      fontWeight: 600,
      fontSize: '14px',
    },

    dividerLine: { backgroundColor: '#E2E8F0' },
    dividerText: { color: '#94A3B8', fontSize: '13px' },

    // Campos de formulario: igual que <Field />/<Input />
    formFieldLabel: {
      fontSize: '14px',
      fontWeight: 600,
      color: '#334155',
    },
    formFieldInput: {
      height: '42px',
      borderRadius: '8px',
      borderColor: '#E2E8F0',
      fontSize: '14px',
      '&:focus': {
        borderColor: '#2563EB',
        boxShadow: '0 0 0 3px rgba(37, 99, 235, 0.12)',
      },
    },
    formFieldHintText: { color: '#64748B', fontSize: '12px' },
    formFieldErrorText: { color: '#DC2626', fontSize: '12px' },
    formFieldSuccessText: { color: '#16A34A', fontSize: '12px' },
    formResendCodeLink: { color: '#2563EB', fontWeight: 600 },

    formButtonPrimary: {
      height: '40px',
      borderRadius: '8px',
      backgroundColor: '#0B2545',
      fontWeight: 600,
      fontSize: '14px',
      '&:hover': { backgroundColor: '#13315C' },
    },

    footerActionText: { color: '#64748B', fontSize: '13px' },
    footerActionLink: {
      color: '#2563EB',
      fontWeight: 600,
      '&:hover': { color: '#2563EB' },
    },

    identityPreviewText: { color: '#0F172A' },
    identityPreviewEditButtonIcon: { color: '#2563EB' },

    // Popover del UserButton (esquina superior derecha)
    userButtonPopoverCard: {
      borderRadius: '12px',
      boxShadow: '0 10px 30px rgba(11,37,69,0.10), 0 2px 6px rgba(11,37,69,0.06)',
      border: '1px solid #E2E8F0',
    },
    userButtonPopoverActionButton: {
      fontSize: '14px',
      color: '#334155',
      '&:hover': { backgroundColor: '#F8FAFC' },
    },
  },
};

function AppWithAuth() {
  const { getToken } = useAuth();
  setupApiClient(() => getToken({ template: 'reputaly-backend' }));
  return (
    <ToastProvider>
      <App />
    </ToastProvider>
  );
}

function ClerkProviderWithRoutes() {
  const navigate = useNavigate();
  return (
    <ClerkProvider
      publishableKey={publishableKey}
      localization={esES}
      appearance={clerkAppearance}
      signInUrl="/sign-in"
      signUpUrl="/sign-up"
      routerPush={(to) => navigate(to)}
      routerReplace={(to) => navigate(to, { replace: true })}
    >
      <AppWithAuth />
    </ClerkProvider>
  );
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <ClerkProviderWithRoutes />
    </BrowserRouter>
  </StrictMode>,
);
