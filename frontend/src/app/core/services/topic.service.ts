import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../enviroments/enviroment';
import { AuthService } from './auth.service';

export interface Document {
  id: number;
  title: string;
  type: 'pdf' | 'doc' | 'link';
  url: string;
  size?: string;
}

export interface Contact {
  id: number;
  name: string;
  role: string;
  email: string;
  phone: string;
  department: string;
}

export interface Topic {
  id: number;
  title: string;
  description: string;
  category: string;
  estimatedTime: string;
  documents: Document[];
  links: Document[];
  contacts: Contact[];
  isCompleted: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class TopicService {
  private topicsSubject = new BehaviorSubject<Topic[]>([
    // DADOS MOCADOS REMOVIDOS - AGORA CARREGA DA API
  ]);

  public topics$ = this.topicsSubject.asObservable();

  constructor(private http: HttpClient, private authService: AuthService) {
    // CLEAR ALL MOCKED DATA FROM LOCALSTORAGE
    this.clearAllStoredData();
    // LOAD TOPICS FROM API
    this.loadTopicsFromApi();
  }

  getTopics(): Observable<Topic[]> {
    return this.topics$;
  }

  getTopicById(id: number): Observable<Topic | null> {
    return new Observable(observer => {
      const subscription = this.topics$.subscribe(topics => {
        const topic = topics.find(t => t.id === id) || null;
        observer.next(topic);
      });
      return () => subscription.unsubscribe();
    });
  }

  markTopicAsCompleted(topicId: number, userId?: string): void {
    const currentUser = this.authService.currentUserValue;
    const memberId = userId || currentUser?.email;

    if (!memberId) {
      console.error('❌ Usuário não encontrado para salvar progresso');
      return;
    }

    // Atualiza o estado local primeiro
    const currentTopics = this.topicsSubject.value;
    const updatedTopics = currentTopics.map(topic => {
      if (topic.id === topicId) {
        return { ...topic, isCompleted: true };
      }
      return topic;
    });

    this.topicsSubject.next(updatedTopics);
    this.saveTopicsToStorage(updatedTopics, userId);

    // Envia para a API
    const apiUrl = `${environment.apiUrl}/progress/member/${memberId}/topic/${topicId}/complete`;
    this.http.post(apiUrl, {}).subscribe({
      next: (response) => {
        console.log('✅ Progresso salvo no banco de dados:', response);
      },
      error: (error) => {
        console.error('❌ Erro ao salvar progresso no banco:', error);
        // Reverte o estado local em caso de erro
        const revertedTopics = currentTopics.map(topic => {
          if (topic.id === topicId) {
            return { ...topic, isCompleted: false };
          }
          return topic;
        });
        this.topicsSubject.next(revertedTopics);
        this.saveTopicsToStorage(revertedTopics, userId);
      }
    });
  }

  getCompletionStats(userId?: string): { completed: number; total: number; percentage: number } {
    const topics = this.topicsSubject.value;
    const completed = topics.filter(t => t.isCompleted).length;
    const total = topics.length;
    const percentage = total > 0 ? Math.round((completed / total) * 100) : 0;

    return { completed, total, percentage };
  }

  getCompletionStatsForUser(userId: string): { completed: number; total: number; percentage: number } {
    const userTopics = this.loadTopicsFromStorage(userId);
    const completed = userTopics.filter(t => t.isCompleted).length;
    const total = userTopics.length;
    const percentage = total > 0 ? Math.round((completed / total) * 100) : 0;

    return { completed, total, percentage };
  }

  resetAllTopics(): void {
    const currentTopics = this.topicsSubject.value;
    const resetTopics = currentTopics.map(topic => ({
      ...topic,
      isCompleted: false
    }));

    this.topicsSubject.next(resetTopics);
    this.saveTopicsToStorage(resetTopics);
  }

  private loadTopicsFromStorage(userId?: string): Topic[] {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        const key = userId ? `topics_${userId}` : 'topics';
        const stored = localStorage.getItem(key);
        if (stored) {
          return JSON.parse(stored);
        }
      } catch (error) {
        console.error('Error loading topics from storage:', error);
      }
    }
    // Return default topics if no stored data
    return this.topicsSubject.value;
  }

  private saveTopicsToStorage(topics: Topic[], userId?: string): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        const key = userId ? `topics_${userId}` : 'topics';
        localStorage.setItem(key, JSON.stringify(topics));
      } catch (error) {
        console.error('Error saving topics to storage:', error);
      }
    }
  }

  loadUserTopics(userId: string): void {
    const userTopics = this.loadTopicsFromStorage(userId);
    this.topicsSubject.next(userTopics);
  }

  getAllTopicsForUser(userId: string): import('./dashboard.service').MemberTopicProgress[] {
    const userTopics = this.loadTopicsFromStorage(userId);
    return userTopics.map(topic => ({
      topicId: topic.id,
      topicTitle: topic.title,
      category: topic.category,
      estimatedTime: topic.estimatedTime,
      isCompleted: topic.isCompleted,
      completedDate: topic.isCompleted ? new Date().toISOString().split('T')[0] : undefined,
      timeSpent: topic.isCompleted ? topic.estimatedTime : undefined
    }));
  }

  getTopicsByCategory(category: string): Observable<Topic[]> {
    return new Observable(observer => {
      const subscription = this.topics$.subscribe(topics => {
        const filteredTopics = topics.filter(topic =>
          topic.category.toLowerCase() === category.toLowerCase()
        );
        observer.next(filteredTopics);
      });
      return () => subscription.unsubscribe();
    });
  }

  getAvailableCategories(): Observable<string[]> {
    return new Observable(observer => {
      const subscription = this.topics$.subscribe(topics => {
        const categories = [...new Set(topics.map(topic => topic.category))];
        observer.next(categories.sort());
      });
      return () => subscription.unsubscribe();
    });
  }

  searchTopics(searchTerm: string, category?: string): Observable<Topic[]> {
    return new Observable(observer => {
      const subscription = this.topics$.subscribe(topics => {
        let filteredTopics = topics;

        // Filter by category if specified
        if (category && category !== 'all') {
          filteredTopics = filteredTopics.filter(topic =>
            topic.category.toLowerCase() === category.toLowerCase()
          );
        }

        // Filter by search term
        if (searchTerm && searchTerm.trim() !== '') {
          const term = searchTerm.toLowerCase();
          filteredTopics = filteredTopics.filter(topic =>
            topic.title.toLowerCase().includes(term) ||
            topic.description.toLowerCase().includes(term) ||
            topic.category.toLowerCase().includes(term)
          );
        }

        observer.next(filteredTopics);
      });
      return () => subscription.unsubscribe();
    });
  }

  private loadTopicsFromApi(): void {
    const currentUser = this.authService.currentUserValue;

    if (!currentUser?.email) {
      console.log('⏳ Aguardando usuário estar logado para carregar tópicos...');
      // Se não tem usuário, tenta novamente em 1 segundo
      setTimeout(() => this.loadTopicsFromApi(), 1000);
      return;
    }

    // Carrega os tópicos completos com documentos, links e contatos
    const topicsUrl = `${environment.apiUrl}/progress/topics`;
    const memberProgressUrl = `${environment.apiUrl}/progress/member/${currentUser.email}`;

    // Primeiro carrega os tópicos completos
    this.http.get<any[]>(topicsUrl).subscribe({
      next: (topicsData) => {
        // Depois carrega o progresso do usuário
        this.http.get<any>(memberProgressUrl).subscribe({
          next: (memberData) => {
            const progressMap = new Map();
            if (memberData && memberData.topicsProgress) {
              memberData.topicsProgress.forEach((progress: any) => {
                progressMap.set(progress.topicId, progress.isCompleted);
              });
            }

            const topics: Topic[] = topicsData.map(topic => ({
              id: topic.id,
              title: topic.title,
              description: topic.description,
              category: topic.category,
              estimatedTime: topic.estimatedTime,
              isCompleted: progressMap.get(topic.id) || false,
              documents: topic.documents || [],
              links: topic.links || [],
              contacts: topic.contacts || []
            }));

            console.log('🔍 Dados completos carregados:', topics);
            console.log('📄 Exemplo de tópico 3:', topics.find(t => t.id === 3));

            this.topicsSubject.next(topics);
            console.log(`✅ ${topics.length} tópicos carregados com documentos, links e contatos`);
          },
          error: (progressError) => {
            console.log('⚠️ Erro ao carregar progresso, usando tópicos sem progresso:', progressError);
            // Se não conseguir carregar progresso, usa tópicos sem progresso
            const topics: Topic[] = topicsData.map(topic => ({
              id: topic.id,
              title: topic.title,
              description: topic.description,
              category: topic.category,
              estimatedTime: topic.estimatedTime,
              isCompleted: false,
              documents: topic.documents || [],
              links: topic.links || [],
              contacts: topic.contacts || []
            }));
            this.topicsSubject.next(topics);
            console.log(`✅ ${topics.length} tópicos carregados sem progresso específico`);
          }
        });
      },
      error: (error) => {
        console.error('❌ Erro ao carregar tópicos da API:', error);
        this.topicsSubject.next([]);
      }
    });
  }

  private clearAllStoredData(): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        // Clear all topic-related data from localStorage
        const keys = Object.keys(localStorage);
        keys.forEach(key => {
          if (key.startsWith('topics') || key.includes('progress') || key.includes('topic')) {
            localStorage.removeItem(key);
          }
        });
        console.log('🧹 Dados mocados do localStorage foram limpos');
      } catch (error) {
        console.error('Error clearing stored data:', error);
      }
    }
  }
}