import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ChangePostStatusRequest,
  CreatePostRequest,
  Post,
  UpdatePostRequest
} from '../models/post.model';
import {
  Campaign,
  CreateCampaignRequest,
  UpdateCampaignRequest
} from '../models/campaign.model';
import {
  MarkSlotFailedRequest,
  ScheduleRequest,
  ScheduleSlot
} from '../models/schedule-slot.model';
import { CaptionRequest, CaptionResult, ImageRequest, ImageResult } from '../models/ai.model';
import {
  CreateProviderCredentialRequest,
  ProviderCredential,
  SupportedProvider,
  UpdateProviderCredentialRequest
} from '../models/provider-credential.model';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = '/api/v1';

  constructor(private http: HttpClient) {}

  getPosts(): Observable<Post[]> {
    return this.http.get<Post[]>(`${this.baseUrl}/posts`);
  }

  getPost(id: string): Observable<Post> {
    return this.http.get<Post>(`${this.baseUrl}/posts/${id}`);
  }

  createPost(request: CreatePostRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/posts`, request);
  }

  updatePost(id: string, request: UpdatePostRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/posts/${id}`, request);
  }

  deletePost(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/posts/${id}`);
  }

  changePostStatus(request: ChangePostStatusRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/posts/${request.postId}/status`, request);
  }

  getCampaigns(): Observable<Campaign[]> {
    return this.http.get<Campaign[]>(`${this.baseUrl}/campaigns`);
  }

  getCampaign(id: string): Observable<Campaign> {
    return this.http.get<Campaign>(`${this.baseUrl}/campaigns/${id}`);
  }

  createCampaign(request: CreateCampaignRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/campaigns`, request);
  }

  updateCampaign(id: string, request: UpdateCampaignRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/campaigns/${id}`, request);
  }

  deleteCampaign(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/campaigns/${id}`);
  }

  schedulePost(request: ScheduleRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/scheduling/schedule`, request);
  }

  publishSlot(slotId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/scheduling/${slotId}/publish`, null);
  }

  markSlotFailed(slotId: string, request: MarkSlotFailedRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/scheduling/${slotId}/fail`, request);
  }

  getPendingSlots(): Observable<ScheduleSlot[]> {
    return this.http.get<ScheduleSlot[]>(`${this.baseUrl}/scheduling/pending`);
  }

  getCalendarSlots(start: string, end: string): Observable<ScheduleSlot[]> {
    const params = new HttpParams().set('start', start).set('end', end);
    return this.http.get<ScheduleSlot[]>(`${this.baseUrl}/scheduling/calendar`, { params });
  }

  getSlotsByPost(postId: string): Observable<ScheduleSlot[]> {
    return this.http.get<ScheduleSlot[]>(`${this.baseUrl}/scheduling/by-post/${postId}`);
  }

  generateCaption(request: CaptionRequest): Observable<CaptionResult> {
    return this.http.post<CaptionResult>(`${this.baseUrl}/ai/caption`, request);
  }

  generateImage(request: ImageRequest): Observable<ImageResult> {
    return this.http.post<ImageResult>(`${this.baseUrl}/ai/image`, request);
  }

  getProviderCredentials(): Observable<ProviderCredential[]> {
    return this.http.get<ProviderCredential[]>(`${this.baseUrl}/provider-credentials`);
  }

  getProviderCredential(id: string): Observable<ProviderCredential> {
    return this.http.get<ProviderCredential>(`${this.baseUrl}/provider-credentials/${id}`);
  }

  createProviderCredential(request: CreateProviderCredentialRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/provider-credentials`, request);
  }

  updateProviderCredential(id: string, request: UpdateProviderCredentialRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/provider-credentials/${id}`, request);
  }

  deleteProviderCredential(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/provider-credentials/${id}`);
  }

  validateProviderCredential(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/provider-credentials/${id}/validate`, null);
  }

  getSupportedProviders(): Observable<SupportedProvider[]> {
    return this.http.get<SupportedProvider[]>(`${this.baseUrl}/provider-credentials/supported`);
  }
}