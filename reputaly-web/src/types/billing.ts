// La forma del estado de facturación que devuelve GET /billing/status.
// Cada campo se corresponde 1:1 con el BillingStatusDto del backend.
export interface BillingStatus {
// Plna actual del tenant: "free", "stater", "pro"
plan: string;

// Limite de ubicaciones del plan.
maxLocations: number;

//Respuestas IA al mes que permite el plan.
monthlyAiReplies: number;

// Respuesta IA ya usadas este mes.
aiRepliesUsed: number;

// Si el plan permite auto-respuesta con IA.
canAutoReply: boolean;

//Fecha de proxima renovacion. Null si no hay subscripcion activa(Free).
currentPeriodEnd: string | null;

// True si la subscripcion esta marcar para cancelarse al final del periodo actual.
cancelAtPeriodEnd: boolean;
}