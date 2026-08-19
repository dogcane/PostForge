export interface CaptionRequest {
  brief: string;
  platform?: string;
  tone?: string;
}

export interface CaptionResult {
  caption: string;
}

export interface ImageRequest {
  prompt: string;
  style?: string;
}

export interface ImageResult {
  blobUri: string;
  prompt?: string;
}