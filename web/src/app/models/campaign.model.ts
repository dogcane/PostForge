export enum CampaignGoal {
  Awareness = 0,
  Reputation = 1,
  LeadGeneration = 2
}

export enum CampaignChannel {
  Organic = 0,
  Paid = 1
}

export interface Campaign {
  id: string;
  name: string;
  goal: CampaignGoal;
  channel: CampaignChannel;
  startDateUtc: string;
  endDateUtc?: string;
  postIds: string[];
  createdAtUtc: string;
}

export interface CreateCampaignRequest {
  name: string;
  goal: CampaignGoal;
  channel: CampaignChannel;
  startDateUtc: string;
  endDateUtc?: string;
}

export interface UpdateCampaignRequest extends CreateCampaignRequest {
  id: string;
}

export function campaignGoalLabel(goal: CampaignGoal): string {
  switch (goal) {
    case CampaignGoal.Awareness: return 'Awareness';
    case CampaignGoal.Reputation: return 'Reputation';
    case CampaignGoal.LeadGeneration: return 'Lead Gen';
    default: return String(goal);
  }
}

export function campaignGoalClass(goal: CampaignGoal): string {
  switch (goal) {
    case CampaignGoal.Awareness: return 'pf-goal--awareness';
    case CampaignGoal.Reputation: return 'pf-goal--reputation';
    case CampaignGoal.LeadGeneration: return 'pf-goal--leadgeneration';
    default: return '';
  }
}

export function campaignChannelLabel(channel: CampaignChannel): string {
  switch (channel) {
    case CampaignChannel.Organic: return 'Organic';
    case CampaignChannel.Paid: return 'Paid';
    default: return String(channel);
  }
}

export function campaignChannelClass(channel: CampaignChannel): string {
  return 'pf-channel--' + campaignChannelLabel(channel).toLowerCase();
}