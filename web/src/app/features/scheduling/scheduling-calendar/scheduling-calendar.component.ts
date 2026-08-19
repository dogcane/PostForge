import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { ScheduleSlot } from '../../../models/schedule-slot.model';
import { postStatusClass, postStatusLabel } from '../../../models/post.model';
import { platformBadgeClass, platformIcon, platformLabel } from '../../../models/platform.model';
import { ApiService } from '../../../services/api.service';

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

      <div class="pf-alert" *ngIf="error">{{ error }}</div>

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
            <span class="pf-badge pf-badge--sm" *ngFor="let slot of day.slots" [class]="platformBadgeClass(slot.platform)">
              <mat-icon>{{ platformIcon(slot.platform) }}</mat-icon>
              {{ platformLabel(slot.platform) }}
            </span>
          </div>
        </div>
      </div>

      <h2 class="pf-section-title">Pending scheduled posts</h2>
      <div class="pf-upcoming">
        <ng-container *ngIf="!loading; else loadingState">
          <mat-card class="pf-card pf-slot-row" *ngFor="let slot of upcomingSlots">
            <span class="pf-status" [class]="postStatusClass(slot.status)">{{ postStatusLabel(slot.status) }}</span>
            <span class="pf-badge" [class]="platformBadgeClass(slot.platform)">
              <mat-icon>{{ platformIcon(slot.platform) }}</mat-icon>
              {{ platformLabel(slot.platform) }}
            </span>
            <span class="pf-slot-row__time">
              <mat-icon>schedule</mat-icon>
              {{ slot.scheduledAtUtc | date: 'MMM d, yyyy · h:mm a' }}
            </span>
            <span class="pf-slot-row__retry" *ngIf="slot.retryCount > 0">
              retry #{{ slot.retryCount }}
            </span>
          </mat-card>

          <div class="pf-empty" *ngIf="upcomingSlots.length === 0">
            <mat-icon>event_note</mat-icon>
            <h3>Nothing scheduled</h3>
            <p>Create a post and schedule it to appear here.</p>
          </div>
        </ng-container>
      </div>

      <ng-template #loadingState>
        <div class="pf-loading"><mat-icon>autorenew</mat-icon> Loading scheduled posts...</div>
      </ng-template>
    </div>
  `
})
export class SchedulingCalendarComponent implements OnInit {
  currentMonth: Date = new Date();
  dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  calendarDays: { number: number; otherMonth: boolean; isToday: boolean; slots: ScheduleSlot[] }[] = [];
  monthSlots: ScheduleSlot[] = [];
  upcomingSlots: ScheduleSlot[] = [];
  loading = false;
  error: string | null = null;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadMonthSlots();
    this.loadPendingSlots();
  }

  private loadMonthSlots(): void {
    this.loading = true;
    this.error = null;
    const start = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth(), 1);
    const end = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() + 1, 1);
    this.api.getCalendarSlots(start.toISOString(), end.toISOString()).subscribe({
      next: (slots) => {
        this.monthSlots = slots;
        this.loading = false;
        this.buildCalendar();
      },
      error: () => {
        this.error = 'Unable to load scheduled posts.';
        this.loading = false;
      }
    });
  }

  private loadPendingSlots(): void {
    this.api.getPendingSlots().subscribe({
      next: (slots) => {
        this.upcomingSlots = [...slots].sort(
          (a, b) => new Date(a.scheduledAtUtc).getTime() - new Date(b.scheduledAtUtc).getTime()
        );
      },
      error: () => {
        if (!this.error) {
          this.error = 'Unable to load the publishing queue.';
        }
      }
    });
  }

  get currentMonthName(): string {
    return this.currentMonth.toLocaleString('default', { month: 'long' });
  }

  get currentYear(): number {
    return this.currentMonth.getFullYear();
  }

  previousMonth(): void {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() - 1, 1);
    this.loadMonthSlots();
  }

  nextMonth(): void {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() + 1, 1);
    this.loadMonthSlots();
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
    for (const slot of this.monthSlots) {
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

  readonly postStatusClass = postStatusClass;
  readonly postStatusLabel = postStatusLabel;
  readonly platformBadgeClass = platformBadgeClass;
  readonly platformIcon = platformIcon;
  readonly platformLabel = platformLabel;
}