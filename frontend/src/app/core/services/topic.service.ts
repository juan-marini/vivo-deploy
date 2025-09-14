import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

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
    {
      id: 1,
      title: 'SQL Server',
      description: 'Banco de dados principal utilizado para armazenar dados de clientes e transações. Aprenda sobre configuração, otimização e melhores práticas.',
      category: 'Banco de Dados',
      estimatedTime: '2h',
      isCompleted: false,
      documents: [
        { id: 1, title: 'Manual SQL Server.pdf', type: 'pdf', url: '#', size: '2.5 MB' },
        { id: 2, title: 'Guia de Consultas.pdf', type: 'pdf', url: '#', size: '1.8 MB' },
        { id: 3, title: 'Configuração de Índices.doc', type: 'doc', url: '#', size: '850 KB' }
      ],
      links: [
        { id: 1, title: 'Portal de Documentação Interna', type: 'link', url: 'https://docs.vivo.com/sql' },
        { id: 2, title: 'Tutorial SQL Server Microsoft', type: 'link', url: 'https://docs.microsoft.com/sql' }
      ],
      contacts: [
        {
          id: 1,
          name: 'Ana Silva',
          role: 'DBA Senior',
          email: 'ana.silva@vivo.com',
          phone: 'Ramal: 1234',
          department: 'Infraestrutura'
        }
      ]
    },
    {
      id: 2,
      title: 'Oracle',
      description: 'Banco de dados secundário utilizado para sistemas específicos e data warehouse.',
      category: 'Banco de Dados',
      estimatedTime: '1.5h',
      isCompleted: false,
      documents: [
        { id: 1, title: 'Oracle Setup Guide.pdf', type: 'pdf', url: '#', size: '3.2 MB' },
        { id: 2, title: 'Query Optimization.pdf', type: 'pdf', url: '#', size: '2.1 MB' }
      ],
      links: [
        { id: 1, title: 'Oracle Documentation', type: 'link', url: 'https://docs.oracle.com' }
      ],
      contacts: [
        {
          id: 1,
          name: 'Carlos Santos',
          role: 'DBA Oracle',
          email: 'carlos.santos@vivo.com',
          phone: 'Ramal: 1567',
          department: 'Infraestrutura'
        }
      ]
    },
    {
      id: 3,
      title: 'MongoDB',
      description: 'Banco de dados NoSQL para projetos específicos',
      category: 'Banco de Dados',
      estimatedTime: '3h',
      isCompleted: false,
      documents: [
        { id: 1, title: 'MongoDB Guide.pdf', type: 'pdf', url: '#', size: '2.8 MB' }
      ],
      links: [
        { id: 1, title: 'MongoDB Documentation', type: 'link', url: 'https://docs.mongodb.com' }
      ],
      contacts: [
        {
          id: 1,
          name: 'Lucas Pereira',
          role: 'NoSQL Specialist',
          email: 'lucas.pereira@vivo.com',
          phone: 'Ramal: 1890',
          department: 'Desenvolvimento'
        }
      ]
    },
    {
      id: 4,
      title: 'Ferramentas de Desenvolvimento',
      description: 'IDEs, frameworks e bibliotecas utilizadas',
      category: 'Desenvolvimento',
      estimatedTime: '4h',
      isCompleted: false,
      documents: [
        { id: 1, title: 'Guia de Setup.pdf', type: 'pdf', url: '#', size: '1.5 MB' }
      ],
      links: [
        { id: 1, title: 'Documentação Interna', type: 'link', url: 'https://dev.vivo.com' }
      ],
      contacts: [
        {
          id: 1,
          name: 'Mariana Costa',
          role: 'Tech Lead',
          email: 'mariana.costa@vivo.com',
          phone: 'Ramal: 2001',
          department: 'Desenvolvimento'
        }
      ]
    },
    {
      id: 5,
      title: 'Políticas de RH',
      description: 'Diretrizes e procedimentos de recursos humanos',
      category: 'RH',
      estimatedTime: '1h',
      isCompleted: false,
      documents: [
        { id: 1, title: 'Manual do Funcionário.pdf', type: 'pdf', url: '#', size: '3.1 MB' }
      ],
      links: [
        { id: 1, title: 'Portal RH', type: 'link', url: 'https://rh.vivo.com' }
      ],
      contacts: [
        {
          id: 1,
          name: 'Patricia Oliveira',
          role: 'Analista de RH',
          email: 'patricia.oliveira@vivo.com',
          phone: 'Ramal: 3000',
          department: 'Recursos Humanos'
        }
      ]
    },
    {
      id: 6,
      title: 'Infraestrutura AWS',
      description: 'Serviços e configurações da Amazon Web Services',
      category: 'Infraestrutura',
      estimatedTime: '5h',
      isCompleted: false,
      documents: [
        { id: 1, title: 'AWS Setup Guide.pdf', type: 'pdf', url: '#', size: '4.2 MB' }
      ],
      links: [
        { id: 1, title: 'AWS Console', type: 'link', url: 'https://aws.amazon.com' }
      ],
      contacts: [
        {
          id: 1,
          name: 'Roberto Silva',
          role: 'Cloud Architect',
          email: 'roberto.silva@vivo.com',
          phone: 'Ramal: 4000',
          department: 'Infraestrutura'
        }
      ]
    }
  ]);

  public topics$ = this.topicsSubject.asObservable();

  constructor() {
    // Load topics without user ID initially
    const storedTopics = this.loadTopicsFromStorage();
    if (storedTopics.length > 0) {
      this.topicsSubject.next(storedTopics);
    }
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
    const currentTopics = this.topicsSubject.value;
    const updatedTopics = currentTopics.map(topic => {
      if (topic.id === topicId) {
        return { ...topic, isCompleted: true };
      }
      return topic;
    });

    this.topicsSubject.next(updatedTopics);
    this.saveTopicsToStorage(updatedTopics, userId);
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
}