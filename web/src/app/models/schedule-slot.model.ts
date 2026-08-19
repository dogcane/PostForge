import { PostStatus } from './post.model';

export { PostStatus as ScheduleSlotStatus };

export interface ScheduleSlot {
  id: string;
  postId: string;
  platform: string;
  scheduledAtUtc: string;
  status: PostStatus;
  retryCount: number;
  lastError?: string;
  publishedAtUtc?: string;
}

export interface ScheduleRequest {
  postId: string;
  platform: string;
  scheduledAtUtc: string;
}

export interface MarkSlotFailedRequest {
  slotId: string;
  error: string;
}