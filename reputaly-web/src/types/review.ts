export type ReviewStatus =
| 'pending'
| 'auto_replied'
| 'escalated'
| 'replied'
| 'ignored';

// Espejo del ReviewDTo del backend.
// Los campos son | null corresponden a os nullable de c#
// las fechs llegan como string en formato ISO
export interface Review {
    id: string;
    authorName: string;
    rating: number;
    content: string;
    publishedAt: string;
    status: ReviewStatus;
    aiSuggestedReply: string | null;
    aiDecision : string | null;
    aiDecisionReason: string | null;
    finalReply: string | null;
    repliedAt: string | null;
    escalatedAt: string | null;
    createdAt: string;
    locationId: string;
    detectedLanguage: string | null;
    sentimentScore: number | null;
    detectedTopics: string[] | null;
    autoReplied:boolean;
}

export interface ReviewsPage{
    total: number;
    page: number;
    pageSize: number;
    items: Review[];
}