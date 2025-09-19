import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { TopicService } from './topic.service';

export interface ProgressItem {
  id: number;
  title: string;
  description: string;
  completed: boolean;
  completedDate?: string;
  category: string;
  estimatedTime: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProgressService {
  private progressItemsSubject = new BehaviorSubject<ProgressItem[]>([]);
  public progressItems$ = this.progressItemsSubject.asObservable();

  constructor(private topicService: TopicService) {
    this.clearStoredProgress();
    this.syncWithTopics();
  }

  getProgressItems(): Observable<ProgressItem[]> {
    return this.progressItems$;
  }

  getProgressStats(): Observable<{
    total: number;
    completed: number;
    percentage: number;
    estimatedTimeRemaining: string;
  }> {
    return this.progressItems$.pipe(
      map(items => {
        const total = items.length;
        const completed = items.filter(item => item.completed).length;
        const percentage = total > 0 ? Math.round((completed / total) * 100) : 0;
        const estimatedTimeRemaining = this.calculateEstimatedTimeRemaining(items);

        return {
          total,
          completed,
          percentage,
          estimatedTimeRemaining
        };
      })
    );
  }

  getCompletedItems(): Observable<ProgressItem[]> {
    return this.progressItems$.pipe(
      map(items => items.filter(item => item.completed))
    );
  }

  getPendingItems(): Observable<ProgressItem[]> {
    return this.progressItems$.pipe(
      map(items => items.filter(item => !item.completed))
    );
  }

  markItemAsCompleted(itemId: number): void {
    const currentItems = this.progressItemsSubject.value;
    const updatedItems = currentItems.map(item => {
      if (item.id === itemId && !item.completed) {
        return {
          ...item,
          completed: true,
          completedDate: new Date().toISOString().split('T')[0]
        };
      }
      return item;
    });

    this.progressItemsSubject.next(updatedItems);
    this.saveProgressToStorage(updatedItems);
  }

  markItemAsPending(itemId: number): void {
    const currentItems = this.progressItemsSubject.value;
    const updatedItems = currentItems.map(item => {
      if (item.id === itemId && item.completed) {
        const { completedDate, ...itemWithoutDate } = item;
        return {
          ...itemWithoutDate,
          completed: false
        };
      }
      return item;
    });

    this.progressItemsSubject.next(updatedItems);
    this.saveProgressToStorage(updatedItems);
  }

  private syncWithTopics(): void {
    this.topicService.topics$.subscribe(topics => {
      const progressItems: ProgressItem[] = topics.map(topic => ({
        id: topic.id,
        title: topic.title,
        description: topic.description,
        completed: topic.isCompleted,
        completedDate: topic.isCompleted ? this.getCompletedDate(topic.id) : undefined,
        category: topic.category,
        estimatedTime: topic.estimatedTime
      }));

      this.progressItemsSubject.next(progressItems);
      this.saveProgressToStorage(progressItems);
    });
  }

  private getCompletedDate(topicId: number): string {
    const storedItems = this.getStoredProgress();
    const storedItem = storedItems.find(item => item.id === topicId);
    return storedItem?.completedDate || new Date().toISOString().split('T')[0];
  }

  private calculateEstimatedTimeRemaining(items: ProgressItem[]): string {
    const pendingItems = items.filter(item => !item.completed);
    let totalMinutes = 0;

    pendingItems.forEach(item => {
      const time = item.estimatedTime;
      if (time.includes('h')) {
        const hours = parseFloat(time.replace('h', ''));
        totalMinutes += hours * 60;
      }
    });

    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    if (hours > 0) {
      return minutes > 0 ? `${hours}h ${minutes}min` : `${hours}h`;
    }
    return `${minutes}min`;
  }

  private getStoredProgress(): ProgressItem[] {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        const stored = localStorage.getItem('progressItems');
        return stored ? JSON.parse(stored) : [];
      } catch (error) {
        console.error('Error loading progress from storage:', error);
      }
    }
    return [];
  }

  private saveProgressToStorage(items: ProgressItem[]): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        localStorage.setItem('progressItems', JSON.stringify(items));
      } catch (error) {
        console.error('Error saving progress to storage:', error);
      }
    }
  }

  private clearStoredProgress(): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        localStorage.removeItem('progressItems');
        console.log('🧹 Dados de progresso do localStorage foram limpos');
      } catch (error) {
        console.error('Error clearing progress storage:', error);
      }
    }
  }
}