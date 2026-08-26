import { Routes } from '@angular/router';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { LoginComponent } from './features/auth/login/login.component';
import { PostListComponent } from './features/posts/post-list/post-list.component';
import { PostFormComponent } from './features/posts/post-form/post-form.component';
import { CampaignListComponent } from './features/campaigns/campaign-list/campaign-list.component';
import { CampaignFormComponent } from './features/campaigns/campaign-form/campaign-form.component';
import { SchedulingCalendarComponent } from './features/scheduling/scheduling-calendar/scheduling-calendar.component';
import { AiAssistComponent } from './features/ai/ai-assist/ai-assist.component';
import { TenantListComponent } from './features/tenants/tenant-list/tenant-list.component';
import { TenantFormComponent } from './features/tenants/tenant-form/tenant-form.component';
import { TenantDetailComponent } from './features/tenants/tenant-detail/tenant-detail.component';
import { ProviderListComponent } from './features/provider-credentials/provider-list/provider-list.component';
import { ProviderFormComponent } from './features/provider-credentials/provider-form/provider-form.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'posts', pathMatch: 'full' },
      { path: 'posts', component: PostListComponent },
      { path: 'posts/new', component: PostFormComponent },
      { path: 'posts/:id', component: PostFormComponent },
      { path: 'campaigns', component: CampaignListComponent },
      { path: 'campaigns/new', component: CampaignFormComponent },
      { path: 'campaigns/:id', component: CampaignFormComponent },
      { path: 'scheduling', component: SchedulingCalendarComponent },
      { path: 'ai', component: AiAssistComponent },
      { path: 'tenants', component: TenantListComponent },
      { path: 'tenants/new', component: TenantFormComponent },
      { path: 'tenants/:id', component: TenantDetailComponent },
      { path: 'provider-credentials', component: ProviderListComponent },
      { path: 'provider-credentials/new', component: ProviderFormComponent },
      { path: 'provider-credentials/:id', component: ProviderFormComponent }
    ]
  },
  { path: '**', redirectTo: '' }
];