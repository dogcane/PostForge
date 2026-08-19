export enum PostStatus {
  Draft = 0,
  Ready = 1,
  Scheduled = 2,
  Publishing = 3,
  Published = 4,
  Failed = 5
}

export enum PostTagType {
  Mention = 0,
  UserTag = 1,
  Collaborator = 2
}

export interface MediaAsset {
  id: string;
  tenantId: string;
  blobUri: string;
  mediaType: string;
  generatedByAi: boolean;
  sourcePrompt?: string;
  createdAtUtc: string;
}

export interface PostTag {
  platform: string;
  tagType: PostTagType;
  username: string;
}

export interface Post {
  id: string;
  text: string;
  mediaAssets: MediaAsset[];
  targetPlatforms: string[];
  tags: PostTag[];
  campaignId?: string;
  status: PostStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreatePostRequest {
  text: string;
  mediaAssetIds?: string[];
  targetPlatforms?: string[];
  campaignId?: string;
  tags?: PostTag[];
}

export interface UpdatePostRequest {
  id: string;
  text: string;
  mediaAssets?: MediaAsset[];
  targetPlatforms?: string[];
  tags?: PostTag[];
}

export interface ChangePostStatusRequest {
  postId: string;
  newStatus: PostStatus;
}

const POST_STATUS_LABELS: Record<PostStatus, string> = {
  [PostStatus.Draft]: 'Draft',
  [PostStatus.Ready]: 'Ready',
  [PostStatus.Scheduled]: 'Scheduled',
  [PostStatus.Publishing]: 'Publishing',
  [PostStatus.Published]: 'Published',
  [PostStatus.Failed]: 'Failed'
};

export function postStatusLabel(status: PostStatus): string {
  return POST_STATUS_LABELS[status] ?? String(status);
}

export function postStatusClass(status: PostStatus): string {
  return 'pf-status--' + postStatusLabel(status).toLowerCase();
}

export function postTagTypeLabel(tagType: PostTagType): string {
  switch (tagType) {
    case PostTagType.Mention: return 'Mention';
    case PostTagType.UserTag: return 'Tag on photo';
    case PostTagType.Collaborator: return 'Collaborator';
    default: return String(tagType);
  }
}

export function postTagIcon(tagType: PostTagType): string {
  switch (tagType) {
    case PostTagType.Mention: return 'alternate_email';
    case PostTagType.UserTag: return 'person_pin';
    case PostTagType.Collaborator: return 'group';
    default: return 'person';
  }
}