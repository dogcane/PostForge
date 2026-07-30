import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Post, PostContent } from '../models/post.model';
import { Campaign, CampaignRequest } from '../models/campaign.model';
import { ScheduleSlot, ScheduleRequest } from '../models/schedule-slot.model';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = '/api/v1';

  constructor(private http: HttpClient) {}

  getPosts(): Observable<Post[]> {
    return this.http.get<Post[]>(`${this.baseUrl}/posts`);
  }

  getPost(id: string): Observable<Post> {
    return this.http.get<Post>(`${this.baseUrl}/posts/${id}`);
  }

  createPost(post: PostContent): Observable<Post> {
    return this.http.post<Post>(`${this.baseUrl}/posts`, post);
  }

  updatePost(id: string, post: PostContent): Observable<Post> {
    return this.http.put<Post>(`${this.baseUrl}/posts/${id}`, post);
  }

  deletePost(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/posts/${id}`);
  }

  getCampaigns(): Observable<Campaign[]> {
    return this.http.get<Campaign[]>(`${this.baseUrl}/campaigns`);
  }

  getCampaign(id: string): Observable<Campaign> {
    return this.http.get<Campaign>(`${this.baseUrl}/campaigns/${id}`);
  }

  createCampaign(campaign: CampaignRequest): Observable<Campaign> {
    return this.http.post<Campaign>(`${this.baseUrl}/campaigns`, campaign);
  }

  updateCampaign(id: string, campaign: CampaignRequest): Observable<Campaign> {
    return this.http.put<Campaign>(`${this.baseUrl}/campaigns/${id}`, campaign);
  }

  deleteCampaign(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/campaigns/${id}`);
  }

  getScheduleSlots(postId?: string): Observable<ScheduleSlot[]> {
    let params = new HttpParams();
    if (postId) {
      params = params.set('postId', postId);
    }
    return this.http.get<ScheduleSlot[]>(`${this.baseUrl}/scheduling/slots`, { params });
  }

  createScheduleSlot(slot: ScheduleRequest): Observable<ScheduleSlot> {
    return this.http.post<ScheduleSlot>(`${this.baseUrl}/scheduling/slots`, slot);
  }

  getCalendarSlots(start: string, end: string): Observable<ScheduleSlot[]> {
    const params = new HttpParams()
      .set('start', start)
      .set('end', end);
    return this.http.get<ScheduleSlot[]>(`${this.baseUrl}/scheduling/calendar`, { params });
  }
}
