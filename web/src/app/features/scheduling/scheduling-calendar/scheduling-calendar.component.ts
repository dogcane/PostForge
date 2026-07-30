import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatChipsModule } from '@angular/material/chips';
import { ScheduleSlot, ScheduleSlotStatus } from '../../../models/schedule-slot.model';

@Component({
  selector: 'app-scheduling-calendar',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatChipsModule
  ],
  template: `
    <div class="calendar-header">
      <h1>Editorial Calendar</h1>
      <div class="calendar-nav">
        <button mat-mini-fab (click)="previousMonth()">
          <mat-icon>chevron_left</mat-icon>
        </button>
        <span class="current-month">{{ currentMonthName }} {{ currentYear }}</span>
        <button mat-mini-fab (click)="nextMonth()">
          <mat-icon>chevron_right</mat-icon>
        </button>
      </div>
    </div>

    <div class="calendar-grid">
      <div class="day-header" *ngFor="let day of dayNames">{{ day }}</div>

      <div class="calendar-day" *ngFor="let day of calendarDays" [class.other-month]="day.otherMonth" [class.today]="day.isToday">
        <div class="day-number">{{ day.number }}</div>
        <div class="day-slots">
          <div class="slot-chip" *ngFor="let slot of day.slots" [class.published]="slot.status === ScheduleSlotStatus.Published" [class.failed]="slot.status === ScheduleSlotStatus.Failed">
            {{ slot.platform | slice:0:4 }}
          </div>
        </div>
      </div>
    </div>

    <div class="upcoming-section">
      <h2>Upcoming Scheduled Posts</h2>
      <mat-card *ngFor="let slot of upcomingSlots" class="slot-card">
        <mat-card-content>
          <div class="slot-info">
            <span class="slot-platform">{{ slot.platform }}</span>
            <span class="slot-time">{{ slot.scheduledAtUtc | date:'MMM d, yyyy h:mm a' }}</span>
            <mat-chip [color]="getStatusColor(slot.status)" selected>{{ slot.status }}</mat-chip>
          </div>
        </mat-card-content>
      </mat-card>
      <div class="empty-state" *ngIf="upcomingSlots.length === 0">
        <p>No scheduled posts yet. Create a post and schedule it to appear here.</p>
      </div>
    </div>
  `,
  styles: [`
    .calendar-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }
    .calendar-header h1 {
      margin: 0;
      font-weight: 500;
    }
    .calendar-nav {
      display: flex;
      align-items: center;
      gap: 12px;
    }
    .current-month {
      font-size: 18px;
      font-weight: 500;
      min-width: 200px;
      text-align: center;
    }
    .calendar-grid {
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      gap: 4px;
      margin-bottom: 24px;
    }
    .day-header {
      text-align: center;
      font-weight: 500;
      padding: 8px;
      color: rgba(0, 0, 0, 0.54);
      font-size: 12px;
      text-transform: uppercase;
    }
    .calendar-day {
      min-height: 100px;
      background: white;
      border-radius: 4px;
      padding: 8px;
      border: 1px solid #e0e0e0;
    }
    .calendar-day.other-month {
      opacity: 0.35;
    }
    .calendar-day.today {
      border-color: #3f51b5;
      background: rgba(63, 81, 181, 0.04);
    }
    .day-number {
      font-size: 14px;
      font-weight: 500;
      margin-bottom: 4px;
    }
    .calendar-day.today .day-number {
      color: #3f51b5;
    }
    .day-slots {
      display: flex;
      flex-wrap: wrap;
      gap: 2px;
    }
    .slot-chip {
      background: #e8eaf6;
      border-radius: 8px;
      padding: 2px 6px;
      font-size: 10px;
      color: #283593;
      cursor: pointer;
    }
    .slot-chip.published {
      background: #c8e6c9;
      color: #1b5e20;
    }
    .slot-chip.failed {
      background: #ffcdd2;
      color: #b71c1c;
    }
    .upcoming-section h2 {
      font-weight: 500;
      margin-bottom: 16px;
    }
    .slot-card {
      margin-bottom: 8px;
    }
    .slot-info {
      display: flex;
      align-items: center;
      gap: 16px;
    }
    .slot-platform {
      font-weight: 500;
      min-width: 100px;
    }
    .slot-time {
      color: rgba(0, 0, 0, 0.6);
      flex: 1;
    }
    .empty-state {
      text-align: center;
      padding: 32px;
      color: rgba(0, 0, 0, 0.4);
    }
  `]
})
export class SchedulingCalendarComponent {
  protected readonly ScheduleSlotStatus = ScheduleSlotStatus;

  currentMonth: Date = new Date();
  dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  calendarDays: { number: number; otherMonth: boolean; isToday: boolean; slots: ScheduleSlot[] }[] = [];
  upcomingSlots: ScheduleSlot[] = [];

  get currentMonthName(): string {
    return this.currentMonth.toLocaleString('default', { month: 'long' });
  }

  get currentYear(): number {
    return this.currentMonth.getFullYear();
  }

  constructor() {
    this.buildCalendar();
  }

  previousMonth(): void {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() - 1, 1);
    this.buildCalendar();
  }

  nextMonth(): void {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() + 1, 1);
    this.buildCalendar();
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
  }

  getStatusColor(status: ScheduleSlotStatus): string {
    switch (status) {
      case ScheduleSlotStatus.Published: return 'accent';
      case ScheduleSlotStatus.Failed: return 'warn';
      case ScheduleSlotStatus.Publishing: return 'primary';
      default: return '';
    }
  }
}
