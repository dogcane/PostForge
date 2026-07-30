export enum ScheduleSlotStatus {
  Draft = 'Draft',
  Ready = 'Ready',
  Scheduled = 'Scheduled',
  Publishing = 'Publishing',
  Published = 'Published',
  Failed = 'Failed'
}

export interface ScheduleSlot {
  id: string;
  postId: string;
  platform: string;
  scheduledAtUtc: string;
  status: ScheduleSlotStatus;
  retryCount: number;
  publishedAt?: string;
  errorMessage?: string;
}

export interface ScheduleRequest {
  postId: string;
  platform: string;
  scheduledAtUtc: string;
}
