import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProgressService, ProgressItem } from '../../core/services/progress.service';
import { TopicService } from '../../core/services/topic.service';
import { AuthService } from '../../core/services/auth.service';
import { HeaderComponent } from '../../shared/components/header/header.component';

@Component({
  selector: 'app-progresso',
  standalone: true,
  imports: [CommonModule, RouterModule, HeaderComponent],
  templateUrl: './progresso.component.html',
  styleUrls: ['./progresso.component.scss']
})
export class ProgressoComponent implements OnInit {
  progressItems: ProgressItem[] = [];
  progressStats = {
    total: 0,
    completed: 0,
    percentage: 0,
    estimatedTimeRemaining: '0h'
  };

  constructor(
    private progressService: ProgressService,
    private topicService: TopicService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadUserTopics();
    this.loadProgressData();
  }

  loadUserTopics() {
    // Carregar tópicos específicos do usuário logado
    const currentUser = this.authService.currentUserValue;
    const userId = currentUser?.email || 'default';

    // Carregar tópicos individuais do usuário
    this.topicService.loadUserTopics(userId);
  }

  loadProgressData() {
    this.progressService.getProgressItems().subscribe(items => {
      this.progressItems = items;
    });

    this.progressService.getProgressStats().subscribe(stats => {
      this.progressStats = stats;
    });
  }

  get totalItems(): number {
    return this.progressStats.total;
  }

  get completedItems(): number {
    return this.progressStats.completed;
  }

  get progressPercentage(): number {
    return this.progressStats.percentage;
  }

  get completedItemsList(): ProgressItem[] {
    return this.progressItems.filter(item => item.completed);
  }

  get pendingItemsList(): ProgressItem[] {
    return this.progressItems.filter(item => !item.completed);
  }

  get estimatedTimeRemaining(): string {
    return this.progressStats.estimatedTimeRemaining;
  }

}