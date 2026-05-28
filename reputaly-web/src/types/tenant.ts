export interface Tenant {
  id: string;
  name: string;
  subscriptionPlan: string;
  createdAt: string;
}

export interface TenantSettings {
  autoReplyMinRating: number;
  escalateOnKeywords: string[];
  escalateIfNoReplyHours: number;
  aiPersonality: string;
  notificationEmail: string | null;
}