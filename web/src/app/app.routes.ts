import { Routes } from '@angular/router';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { PostListComponent } from './features/posts/post-list/post-list.component';
import { PostFormComponent } from './features/posts/post-form/post-form.component';
import { CampaignListComponent } from './features/campaigns/campaign-list/campaign-list.component';
import { CampaignFormComponent } from './features/campaigns/campaign-form/campaign-form.component';
import { SchedulingCalendarComponent } from './features/scheduling/scheduling-calendar/scheduling-calendar.component';
import { AiAssistComponent } from './features/ai/ai-assist/ai-assist.component';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', redirectTo: 'posts', pathMatch: 'full' },
      { path: 'posts', component: PostListComponent },
      { path: 'posts/new', component: PostFormComponent },
      { path: 'posts/:id', component: PostFormComponent },
      { path: 'campaigns', component: CampaignListComponent },
      { path: 'campaigns/new', component: CampaignFormComponent },
      { path: 'campaigns/:id', component: CampaignFormComponent },
      { path: 'scheduling', component: SchedulingCalendarComponent },
      { path: 'ai', component: AiAssistComponent }
    ]
  }
];
