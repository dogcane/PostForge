export interface PlatformInfo {
  key: string;
  label: string;
  icon: string;
}

export const PLATFORM_OPTIONS: PlatformInfo[] = [
  { key: 'FACEBOOK', label: 'Facebook', icon: 'thumb_up' },
  { key: 'INSTAGRAM', label: 'Instagram', icon: 'photo_camera' },
  { key: 'TIKTOK', label: 'TikTok', icon: 'music_note' },
  { key: 'YOUTUBE', label: 'YouTube', icon: 'play_circle' }
];

const PLATFORM_INDEX: Record<string, PlatformInfo> = PLATFORM_OPTIONS.reduce(
  (acc, p) => {
    acc[p.key] = p;
    return acc;
  },
  {} as Record<string, PlatformInfo>
);

export function platformLabel(platform: string): string {
  return PLATFORM_INDEX[platform.toUpperCase()]?.label ?? platform;
}

export function platformIcon(platform: string): string {
  return PLATFORM_INDEX[platform.toUpperCase()]?.icon ?? 'language';
}

export function platformBadgeClass(platform: string): string {
  return 'pf-badge--' + platform.toLowerCase();
}