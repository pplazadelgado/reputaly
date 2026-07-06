export interface StatusBreakdown {
  pending: number;
  autoReplied: number;
  replied: number;
  escalated: number;
}

export interface RatingPoint {
  label: string;
  value: number;
}

export interface TopicCount {
  topic: string;
  count: number;
}

export interface Analytics {
  averageRating: number;
  totalReviews: number;
  starDistribution: number[]; // longitud 5
  statusBreakdown: StatusBreakdown;
  averageResponseTimeHours: number | null;
  ratingEvolution: RatingPoint[];
  averageSentiment: number | null;
  topTopics: TopicCount[];
}

export interface AnalyticsFilters {
  from?: string; // ISO date
  to?: string;
  locationId?: string;
}