import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { Post, PostStatus } from '../../../models/post.model';

@Component({
  selector: 'app-post-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatCardModule
  ],
  template: `
    <div class="post-list-header">
      <h1>Posts</h1>
      <button mat-raised-button color="primary" routerLink="/posts/new">
        <mat-icon>add</mat-icon>
        New Post
      </button>
    </div>

    <mat-card>
      <mat-card-content>
        <table mat-table [dataSource]="posts" class="post-table">
          <ng-container matColumnDef="text">
            <th mat-header-cell *matHeaderCellDef>Content</th>
            <td mat-cell *matCellDef="let post">{{ post.text | slice:0:80 }}{{ post.text.length > 80 ? '...' : '' }}</td>
          </ng-container>

          <ng-container matColumnDef="platforms">
            <th mat-header-cell *matHeaderCellDef>Platforms</th>
            <td mat-cell *matCellDef="let post">
              <mat-chip-set>
                <mat-chip *ngFor="let platform of post.targetPlatforms">{{ platform }}</mat-chip>
              </mat-chip-set>
            </td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let post">
              <mat-chip [color]="getStatusColor(post.status)" selected>{{ post.status }}</mat-chip>
            </td>
          </ng-container>

          <ng-container matColumnDef="createdAt">
            <th mat-header-cell *matHeaderCellDef>Created</th>
            <td mat-cell *matCellDef="let post">{{ post.createdAt | date:'medium' }}</td>
          </ng-container>

          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let post">
              <button mat-icon-button [routerLink]="['/posts', post.id]">
                <mat-icon>edit</mat-icon>
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .post-list-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }
    .post-list-header h1 {
      margin: 0;
      font-weight: 500;
    }
    .post-table {
      width: 100%;
    }
  `]
})
export class PostListComponent {
  displayedColumns: string[] = ['text', 'platforms', 'status', 'createdAt', 'actions'];

  posts: Post[] = [];

  getStatusColor(status: PostStatus): string {
    switch (status) {
      case PostStatus.Draft: return '';
      case PostStatus.Ready: return 'accent';
      case PostStatus.Scheduled: return 'primary';
      case PostStatus.Publishing: return 'primary';
      case PostStatus.Published: return 'accent';
      case PostStatus.Failed: return 'warn';
      default: return '';
    }
  }
}
