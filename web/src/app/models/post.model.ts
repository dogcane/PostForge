export enum PostStatus {
  Draft = 'Draft',
  Ready = 'Ready',
  Scheduled = 'Scheduled',
  Publishing = 'Publishing',
  Published = 'Published',
  Failed = 'Failed'
}

export interface Post {
  id: string;
  text: string;
  mediaAssets: MediaAsset[];
  targetPlatforms: string[];
  campaignId?: string;
  status: PostStatus;
  createdAt: string;
  updatedAt: string;
}

export interface MediaAsset {
  id: string;
  blobUri: string;
  type: 'image' | 'video';
  generatedByAi: boolean;
  sourcePrompt?: string;
}

export interface PostContent {
  text: string;
  mediaIds: string[];
  platformKeys: string[];
  campaignId?: string;
  scheduledAt?: string;
}
