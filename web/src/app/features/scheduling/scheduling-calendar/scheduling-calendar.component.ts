import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { ScheduleSlot, ScheduleSlotStatus } from '../../../models/schedule-slot.model';

@Component({
  selector: 'app-scheduling-calendar',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  template: `
    <div class="pf-page">
      <div class="pf-page-header">
        <div>
          <h1 class="pf-title">Editorial Calendar</h1>
          <p class="pf-subtitle">Your publishing plan, at a glance</p>
        </div>

        <div class="pf-calendar__nav">
          <button (click)="previousMonth()" aria-label="Previous month">
            <mat-icon>chevron_left</mat-icon>
          </button>
          <span class="pf-calendar__month">{{ currentMonthName }} {{ currentYear }}</span>
          <button (click)="nextMonth()" aria-label="Next month">
            <mat-icon>chevron_right</mat-icon>
          </button>
        </div>
      </div>

      <div class="pf-calendar__grid">
        <div class="pf-calendar__dow" *ngFor="let day of dayNames">{{ day }}</div>

        <div
          class="pf-calendar__day"
          *ngFor="let day of calendarDays"
          [class.pf-calendar__day--other]="day.otherMonth"
          [class.pf-calendar__day--today]="day.isToday"
        >
          <span class="pf-calendar__num">{{ day.number }}</span>
          <div class="pf-calendar__slots">
            <span class="pf-badge pf-badge--sm" *ngFor="let slot of day.slots" [class]="'pf-badge--' + slot.platform.toLowerCase()">
              <mat-icon>{{ platformIcon(slot.platform) }}</mat-icon>
              {{ slot.platform | slice: 0:4 }}
            </span>
          </div>
        </div>
      </div>

      <h2 class="pf-section-title">Upcoming scheduled posts</h2>
      <div class="pf-upcoming">
        <mat-card class="pf-card pf-slot-row" *ngFor="let slot of upcomingSlots">
          <span class="pf-status" [class]="'pf-status--' + slot.status.toLowerCase()">{{ slot.status }}</span>
          <span class="pf-badge" [class]="'pf-badge--' + slot.platform.toLowerCase()">
            <mat-icon>{{ platformIcon(slot.platform) }}</mat-icon>
            {{ platformLabel(slot.platform) }}
          </span>
          <span class="pf-slot-row__time">
            <mat-icon>schedule</mat-icon>
            {{ slot.scheduledAtUtc | date: 'MMM d, yyyy · h:mm a' }}
          </span>
        </mat-card>

        <div class="pf-empty" *ngIf="upcomingSlots.length === 0">
          <mat-icon>event_note</mat-icon>
          <h3>Nothing scheduled</h3>
          <p>Create a post and schedule it to appear here.</p>
        </div>
      </div>
    </div>
  `
})
export class SchedulingCalendarComponent {
  currentMonth: Date = new Date();
  dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  calendarDays: { number: number; otherMonth: boolean; isToday: boolean; slots: ScheduleSlot[] }[] = [];
  upcomingSlots: ScheduleSlot[] = [
    this.makeSlot('1', 'facebook', 0, 10, 0, ScheduleSlotStatus.Scheduled),
    this.makeSlot('2', 'instagram', 0, 15, 30, ScheduleSlotStatus.Publishing),
    this.makeSlot('3', 'youtube', 1, 9, 0, ScheduleSlotStatus.Scheduled),
    this.makeSlot('4', 'tiktok', 3, 18, 0, ScheduleSlotStatus.Ready),
    this.makeSlot('5', 'facebook', -1, 18, 0, ScheduleSlotStatus.Published)
  ];

  constructor() {
    this.buildCalendar();
  }

  get currentMonthName(): string {
    return this.currentMonth.toLocaleString('default', { month: 'long' });
  }

  get currentYear(): number {
    return this.currentMonth.getFullYear();
  }

  private makeSlot(
    id: string,
    platform: string,
    dayOffset: number,
    hours: number,
    minutes: number,
    status: ScheduleSlotStatus
  ): ScheduleSlot {
    const d = new Date();
    d.setDate(d.getDate() + dayOffset);
    d.setHours(hours, minutes, 0, 0);
    return {
      id,
      postId: id,
      platform,
      scheduledAtUtc: d.toISOString(),
      status,
      retryCount: 0
    };
  }

  previousMonth(): void {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() - 1, 1);
    this.buildCalendar();
  }

  nextMonth(): void {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() + 1, 1);
    this.buildCalendar();
  }

  platformIcon(platform: string): string {
    switch (platform.toLowerCase()) {
      case 'facebook': return 'thumb_up';
      case 'instagram': return 'photo_camera';
      case 'tiktok': return 'music_note';
      case 'youtube': return 'play_circle';
      default: return 'language';
    }
  }

  platformLabel(platform: string): string {
    switch (platform.toLowerCase()) {
      case 'facebook': return 'Facebook';
      case 'instagram': return 'Instagram';
      case 'tiktok': return 'TikTok';
      case 'youtube': return 'YouTube';
      default: return platform;
    }
  }

  private buildCalendar(): void {
    const year = this.currentMonth.getFullYear();
    const month = this.currentMonth.getMonth();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const startDay = firstDay.getDay();
    const today = new Date();

    this.calendarDays = [];

    for (let i = 0; i < startDay; i++) {
      const prevDate = new Date(year, month, -startDay + i + 1);
      this.calendarDays.push({
        number: prevDate.getDate(),
        otherMonth: true,
        isToday: false,
        slots: []
      });
    }

    for (let i = 1; i <= lastDay.getDate(); i++) {
      const date = new Date(year, month, i);
      this.calendarDays.push({
        number: i,
        otherMonth: false,
        isToday: date.toDateString() === today.toDateString(),
        slots: []
      });
    }

    const remaining = 42 - this.calendarDays.length;
    for (let i = 1; i <= remaining; i++) {
      this.calendarDays.push({
        number: i,
        otherMonth: true,
        isToday: false,
        slots: []
      });
    }

    this.assignSlots();
  }

  private assignSlots(): void {
    for (const slot of this.upcomingSlots) {
      const d = new Date(slot.scheduledAtUtc);
      const day = this.calendarDays.find(
        (x) =>
          !x.otherMonth &&
          x.number === d.getDate() &&
          d.getMonth() === this.currentMonth.getMonth() &&
          d.getFullYear() === this.currentMonth.getFullYear()
      );
      if (day) {
        day.slots.push(slot);
      }
    }
  }
}
