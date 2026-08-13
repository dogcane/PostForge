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
  tags: PostTag[];
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

export enum PostTagType {
  Mention = 'Mention',
  UserTag = 'UserTag',
  Collaborator = 'Collaborator'
}

export interface PostTag {
  platform: string;
  tagType: PostTagType;
  username: string;
}

export interface PostContent {
  text: string;
  mediaIds: string[];
  platformKeys: string[];
  campaignId?: string;
  tags?: PostTag[];
  scheduledAt?: string;
}
