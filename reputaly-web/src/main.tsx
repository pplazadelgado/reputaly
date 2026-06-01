import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { ClerkProvider, useAuth } from '@clerk/clerk-react';
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
    fontFamily: '"Manrope", "Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", system-ui, sans-serif',
    borderRadius: '8px',
  },
  elements: {
    card: {
      boxShadow: '0 10px 30px rgba(11,37,69,0.10), 0 2px 6px rgba(11,37,69,0.06)',
      border: '1px solid #E2E8F0',
    },
    formButtonPrimary: {
      backgroundColor: '#0B2545',
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

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ClerkProvider publishableKey={publishableKey} appearance={clerkAppearance}>
      <AppWithAuth />
    </ClerkProvider>
  </StrictMode>,
);
