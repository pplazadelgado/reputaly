export interface AiConfigEntry {
  instructions: string;
  tone: string;
  maxLength?: number;
}

export interface AiConfig {
  default: AiConfigEntry;
  byRating: {
    [key: string]: AiConfigEntry;
  };
}

export interface Tenant {
  id: string;
  name: string;
  vertical: string | null;
  subscriptionPlan: string;
  createdAt: string;
}

export interface TenantSettings {
  autoReplyMinRating: number;
  escalateOnKeywords: string[];
  escalateIfNoReplyHours: number;
  aiConfig: AiConfig;
  defaultResponseLanguage: string;
  autoDetectLanguage: boolean;
  notificationEmail: string | null;
}