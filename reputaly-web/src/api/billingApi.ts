import apiClient from './apiClient';
import type {BillingStatus } from "../types/billing";

// Respuesta de los endpoints que devuelven una URL de Stripe
// (checkout y portal). El backend responde { url: "https://..." }.
interface StripeUrlResponse {
    url: string;
}

//GET /billing/status - estado de facturacion del tenant actual
export async function getBillingStatus(): Promise<BillingStatus> {
    const response = await apiClient.get<BillingStatus>('/billing/status');
    return response.data;
}

//Post /billing/checkout - inicia el proceso de checkout en Stripe y devuelve la URL a la que redirigir al usuario
export async function createCheckoutSession(plan: string): Promise<string> {
  const response = await apiClient.post<StripeUrlResponse>('/billing/checkout', { plan });
  return response.data.url;
}
// POST /billling/portal - abre el portal de gestion de stripe.
// Devuelve la URL del portal (cambair pago, cancelar, facturas)
export async function createBillingPortalSession(): Promise<string> {
    const response = await apiClient.post<StripeUrlResponse>('/billing/portal');
    return response.data.url;
}
