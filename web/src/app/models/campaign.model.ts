export enum CampaignGoal {
  Awareness = 'Awareness',
  Reputation = 'Reputation',
  LeadGeneration = 'LeadGeneration'
}

export enum CampaignChannel {
  Organic = 'Organic',
  Paid = 'Paid'
}

export interface Campaign {
  id: string;
  name: string;
  description?: string;
  goal: CampaignGoal;
  channel: CampaignChannel;
  startDate: string;
  endDate: string;
  postIds: string[];
  createdAt: string;
  updatedAt: string;
}

export interface CampaignRequest {
  name: string;
  description?: string;
  goal: CampaignGoal;
  channel: CampaignChannel;
  startDate: string;
  endDate: string;
}
