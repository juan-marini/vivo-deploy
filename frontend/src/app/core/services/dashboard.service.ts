import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { TopicService } from './topic.service';

export interface TeamMember {
  id: string;
  name: string;
  role: string;
  startDate: string;
  progress: number;
  status: "Concluído" | "Em andamento" | "Não iniciado" | "Atrasado";
}

export interface MemberTopicProgress {
  topicId: number;
  topicTitle: string;
  category: string;
  estimatedTime: string;
  isCompleted: boolean;
  completedDate?: string;
  timeSpent?: string;
}

export interface MemberDetailedProgress {
  member: TeamMember;
  topicsProgress: MemberTopicProgress[];
  totalTopics: number;
  completedTopics: number;
  inProgressTopics: number;
  notStartedTopics: number;
  totalTimeEstimated: string;
  totalTimeSpent: string;
  averageTopicTime: string;
  recentActivity: ActivityItem[];
  suggestedNextTopics: MemberTopicProgress[];
}

export interface ActivityItem {
  date: string;
  action: string;
  topicTitle: string;
  type: 'completed' | 'started' | 'updated';
}

export interface ChartData {
  label: string;
  value: number;
  color: string;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private teamMembersSubject = new BehaviorSubject<TeamMember[]>([
    {
      id: "joao.silva@vivo.com.br",
      name: "João Silva",
      role: "Desenvolvedor Backend",
      startDate: "01/05/2025",
      progress: 0,
      status: "Não iniciado",
    },
    {
      id: "maria.oliveira@vivo.com.br",
      name: "Maria Oliveira",
      role: "Analista de Dados",
      startDate: "15/05/2025",
      progress: 0,
      status: "Não iniciado",
    },
    {
      id: "carlos.santos@vivo.com.br",
      name: "Carlos Santos",
      role: "Desenvolvedor Frontend",
      startDate: "10/05/2025",
      progress: 0,
      status: "Não iniciado",
    },
  ]);

  public teamMembers$ = this.teamMembersSubject.asObservable();

  constructor(private topicService: TopicService) {
    this.loadMembersFromStorage();
    this.initializeUserTopics();
    this.updateProgressForAllMembers();
  }

  getTeamMembers(): Observable<TeamMember[]> {
    return this.teamMembers$;
  }

  private initializeUserTopics(): void {
    // Initialize topics for all team members
    const members = this.teamMembersSubject.value;
    members.forEach(member => {
      this.topicService.loadUserTopics(member.id);
    });
  }

  private updateProgressForAllMembers(): void {
    const currentMembers = this.teamMembersSubject.value;
    const updatedMembers = currentMembers.map(member => {
      const progressStats = this.topicService.getCompletionStatsForUser(member.id);
      return {
        ...member,
        progress: progressStats.percentage,
        status: this.calculateStatusFromProgress(progressStats.percentage)
      };
    });

    this.teamMembersSubject.next(updatedMembers);
    this.saveMembersToStorage(updatedMembers);
  }

  refreshMemberProgress(): void {
    this.updateProgressForAllMembers();
  }

  getChartData(): Observable<ChartData[]> {
    return new Observable(observer => {
      const subscription = this.teamMembers$.subscribe(members => {
        const statusCounts = this.calculateStatusCounts(members);
        const total = members.length;

        const chartData: ChartData[] = [
          {
            label: "Concluído",
            value: total > 0 ? Math.round((statusCounts.completed / total) * 100) : 0,
            color: "#10B981"
          },
          {
            label: "Em andamento",
            value: total > 0 ? Math.round((statusCounts.inProgress / total) * 100) : 0,
            color: "#8B5CF6"
          },
          {
            label: "Não iniciado",
            value: total > 0 ? Math.round((statusCounts.notStarted / total) * 100) : 0,
            color: "#6B7280"
          },
          {
            label: "Atrasado",
            value: total > 0 ? Math.round((statusCounts.delayed / total) * 100) : 0,
            color: "#EF4444"
          },
        ];

        observer.next(chartData);
      });
      return () => subscription.unsubscribe();
    });
  }

  updateMemberProgress(memberId: string, newProgress: number): void {
    const currentMembers = this.teamMembersSubject.value;
    const updatedMembers = currentMembers.map(member => {
      if (member.id === memberId) {
        const updatedMember = { ...member, progress: newProgress };
        updatedMember.status = this.calculateStatusFromProgress(newProgress);
        return updatedMember;
      }
      return member;
    });

    this.teamMembersSubject.next(updatedMembers);
    this.saveMembersToStorage(updatedMembers);
  }

  // Método removido - membros vêm do ColaboradorService

  getStatsSummary(): Observable<{
    total: number;
    completed: number;
    inProgress: number;
    delayed: number;
  }> {
    return new Observable(observer => {
      const subscription = this.teamMembers$.subscribe(members => {
        const statusCounts = this.calculateStatusCounts(members);
        observer.next({
          total: members.length,
          completed: statusCounts.completed,
          inProgress: statusCounts.inProgress,
          delayed: statusCounts.delayed
        });
      });
      return () => subscription.unsubscribe();
    });
  }

  private calculateStatusCounts(members: TeamMember[]): {
    completed: number;
    inProgress: number;
    notStarted: number;
    delayed: number;
  } {
    return {
      completed: members.filter(m => m.status === "Concluído").length,
      inProgress: members.filter(m => m.status === "Em andamento").length,
      notStarted: members.filter(m => m.status === "Não iniciado").length,
      delayed: members.filter(m => m.status === "Atrasado").length
    };
  }

  private calculateStatusFromProgress(progress: number): TeamMember['status'] {
    if (progress === 100) return "Concluído";
    if (progress === 0) return "Não iniciado";
    if (progress < 50) return "Atrasado";
    return "Em andamento";
  }

  // Métodos de simulação removidos - agora usa progresso real individual

  private loadMembersFromStorage(): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        const stored = localStorage.getItem('teamMembers');
        if (stored) {
          const members = JSON.parse(stored);
          this.teamMembersSubject.next(members);
        }
      } catch (error) {
        console.error('Error loading team members from storage:', error);
      }
    }
  }

  private saveMembersToStorage(members: TeamMember[]): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        localStorage.setItem('teamMembers', JSON.stringify(members));
      } catch (error) {
        console.error('Error saving team members to storage:', error);
      }
    }
  }

  resetAllMembersData(): void {
    // Reset all team members to initial state with 0 progress and "Não iniciado" status
    const resetMembers: TeamMember[] = [
      {
        id: "joao.silva@vivo.com.br",
        name: "João Silva",
        role: "Desenvolvedor Backend",
        startDate: "01/05/2025",
        progress: 0,
        status: "Não iniciado",
      },
      {
        id: "maria.oliveira@vivo.com.br",
        name: "Maria Oliveira",
        role: "Analista de Dados",
        startDate: "15/05/2025",
        progress: 0,
        status: "Não iniciado",
      },
      {
        id: "carlos.santos@vivo.com.br",
        name: "Carlos Santos",
        role: "Desenvolvedor Frontend",
        startDate: "10/05/2025",
        progress: 0,
        status: "Não iniciado",
      },
    ];

    this.teamMembersSubject.next(resetMembers);
    this.saveMembersToStorage(resetMembers);

    // Limpar também os tópicos individuais de cada usuário
    this.clearAllUserTopics();
  }

  getMemberDetailedProgress(memberId: string): Observable<MemberDetailedProgress | null> {
    return new Observable(observer => {
      const member = this.teamMembersSubject.value.find(m => m.id === memberId);
      if (!member) {
        observer.next(null);
        return;
      }

      // Use real data from TopicService instead of simulated data
      const memberTopicsProgress = this.topicService.getAllTopicsForUser(memberId);

      const totalTopics = memberTopicsProgress.length;
      const completedTopics = memberTopicsProgress.filter(t => t.isCompleted).length;
      const inProgressTopics = memberTopicsProgress.filter(t => !t.isCompleted && this.hasStartedTopic(memberId, t.topicId)).length;
      const notStartedTopics = totalTopics - completedTopics - inProgressTopics;

      // Calculate time estimates
      const totalTimeEstimated = this.calculateTotalTime(memberTopicsProgress.map(t => t.estimatedTime));
      const totalTimeSpent = this.calculateTimeSpent(memberId, memberTopicsProgress);
      const averageTopicTime = completedTopics > 0 ? this.calculateAverageTime(totalTimeSpent, completedTopics) : '0min';

      // Get realistic activity
      const recentActivity = this.getRealisticActivity(memberId, memberTopicsProgress);

      // Get suggested next topics (uncompleted topics in order)
      const suggestedNextTopics = memberTopicsProgress
        .filter(t => !t.isCompleted)
        .slice(0, 3);

      const detailedProgress: MemberDetailedProgress = {
        member,
        topicsProgress: memberTopicsProgress,
        totalTopics,
        completedTopics,
        inProgressTopics,
        notStartedTopics,
        totalTimeEstimated,
        totalTimeSpent,
        averageTopicTime,
        recentActivity,
        suggestedNextTopics
      };

      observer.next(detailedProgress);
    });
  }

  private hasStartedTopic(userId: string, topicId: number): boolean {
    // Check if user has any progress on this topic (not completed but has some activity)
    if (typeof window !== 'undefined' && window.localStorage) {
      const activity = this.getRecentActivity(userId);
      return activity.some(a => a.topicTitle.includes(topicId.toString()) && a.type === 'started');
    }
    return false;
  }

  private calculateTotalTime(estimatedTimes: string[]): string {
    let totalMinutes = 0;
    estimatedTimes.forEach(time => {
      const minutes = this.parseTimeToMinutes(time);
      totalMinutes += minutes;
    });
    return this.formatMinutesToTime(totalMinutes);
  }

  private parseTimeToMinutes(time: string): number {
    const match = time.match(/(\d+(?:\.\d+)?)([hm])/i);
    if (!match) return 0;
    const value = parseFloat(match[1]);
    const unit = match[2].toLowerCase();
    return unit === 'h' ? value * 60 : value;
  }

  private formatMinutesToTime(minutes: number): string {
    if (minutes < 60) return `${minutes}min`;
    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;
    return remainingMinutes > 0 ? `${hours}h ${remainingMinutes}min` : `${hours}h`;
  }

  private calculateTimeSpent(userId: string, topics: MemberTopicProgress[]): string {
    // Simulate time spent based on completed topics
    const completedTopics = topics.filter(t => t.isCompleted);
    let totalMinutes = 0;
    completedTopics.forEach(topic => {
      totalMinutes += this.parseTimeToMinutes(topic.estimatedTime);
    });
    return this.formatMinutesToTime(Math.floor(totalMinutes * 0.8)); // Assume 80% efficiency
  }

  private calculateAverageTime(totalTime: string, completedCount: number): string {
    const totalMinutes = this.parseTimeToMinutes(totalTime);
    const avgMinutes = Math.floor(totalMinutes / completedCount);
    return this.formatMinutesToTime(avgMinutes);
  }

  private getRecentActivity(userId: string): ActivityItem[] {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        const stored = localStorage.getItem(`activity_${userId}`);
        if (stored) {
          return JSON.parse(stored).slice(0, 10); // Last 10 activities
        }
      } catch (error) {
        console.error('Error loading activity:', error);
      }
    }

    // Generate some mock activity based on completed topics
    const userTopics = this.topicService.getAllTopicsForUser(userId);
    const completedTopics = userTopics.filter(t => t.isCompleted);

    return completedTopics.slice(0, 5).map((topic, index) => ({
      date: new Date(Date.now() - (index * 24 * 60 * 60 * 1000)).toISOString().split('T')[0],
      action: 'Concluiu o tópico',
      topicTitle: topic.topicTitle,
      type: 'completed' as const
    }));
  }

  addActivity(userId: string, activity: ActivityItem): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        const existing = localStorage.getItem(`activity_${userId}`);
        const activities = existing ? JSON.parse(existing) : [];
        activities.unshift(activity);
        // Keep only last 50 activities
        const trimmed = activities.slice(0, 50);
        localStorage.setItem(`activity_${userId}`, JSON.stringify(trimmed));
      } catch (error) {
        console.error('Error saving activity:', error);
      }
    }
  }

  private clearAllUserTopics(): void {
    const userIds = [
      "joao.silva@vivo.com.br",
      "maria.oliveira@vivo.com.br",
      "carlos.santos@vivo.com.br"
    ];

    // Reset the global topic service state
    this.topicService.resetAllTopics();

    if (typeof window !== 'undefined' && window.localStorage) {
      userIds.forEach(userId => {
        localStorage.removeItem(`topics_${userId}`);
        localStorage.removeItem(`activity_${userId}`);
        localStorage.removeItem(`progress_${userId}`);
      });

      // Clear any other progress-related data
      localStorage.removeItem('progressItems');
      localStorage.removeItem('progressStats');
    }
  }

  // DEPRECATED: Method replaced with real data from TopicService
  /* private getSimulatedMemberData(memberId: string): { topicsProgress: MemberTopicProgress[] } {
    // Simulate different progress for each member
    const baseTopics = [
      { id: 1, title: 'SQL Server', category: 'Banco de Dados', estimatedTime: '2h' },
      { id: 2, title: 'Oracle', category: 'Banco de Dados', estimatedTime: '1.5h' },
      { id: 3, title: 'MongoDB', category: 'Banco de Dados', estimatedTime: '3h' },
      { id: 4, title: 'Angular', category: 'Frontend', estimatedTime: '4h' },
      { id: 5, title: 'React', category: 'Frontend', estimatedTime: '3.5h' },
      { id: 6, title: 'ASP.NET Core', category: 'Backend', estimatedTime: '5h' },
      { id: 7, title: 'Docker', category: 'DevOps', estimatedTime: '2.5h' },
      { id: 8, title: 'Kubernetes', category: 'DevOps', estimatedTime: '6h' },
      { id: 9, title: 'Power BI', category: 'Análise de Dados', estimatedTime: '3h' },
      { id: 10, title: 'Python para Dados', category: 'Análise de Dados', estimatedTime: '4h' }
    ];

    const topicsProgress: MemberTopicProgress[] = baseTopics.map(topic => {
      let isCompleted = false;
      let completedDate = undefined;

      // Different progress for each member
      switch (memberId) {
        case 'maria.oliveira@vivo.com.br':
          // Maria: Analista de Dados - completou tópicos de dados e alguns básicos
          isCompleted = [1, 2, 9, 10].includes(topic.id);
          if (isCompleted) {
            const daysAgo = Math.floor(Math.random() * 30);
            completedDate = new Date(Date.now() - daysAgo * 24 * 60 * 60 * 1000).toISOString().split('T')[0];
          }
          break;

        case 'joao.silva@vivo.com.br':
          // João: Desenvolvedor Backend - completou backend e banco de dados
          isCompleted = [1, 2, 6].includes(topic.id);
          if (isCompleted) {
            const daysAgo = Math.floor(Math.random() * 20);
            completedDate = new Date(Date.now() - daysAgo * 24 * 60 * 60 * 1000).toISOString().split('T')[0];
          }
          break;

        case 'carlos.santos@vivo.com.br':
          // Carlos: Desenvolvedor Frontend - completou frontend e alguns básicos
          isCompleted = [4, 5, 1].includes(topic.id);
          if (isCompleted) {
            const daysAgo = Math.floor(Math.random() * 25);
            completedDate = new Date(Date.now() - daysAgo * 24 * 60 * 60 * 1000).toISOString().split('T')[0];
          }
          break;
      }

      return {
        topicId: topic.id,
        topicTitle: topic.title,
        category: topic.category,
        estimatedTime: topic.estimatedTime,
        isCompleted,
        completedDate,
        timeSpent: isCompleted ? topic.estimatedTime : undefined
      };
    });

    return { topicsProgress };
  } */

  private getRealisticActivity(memberId: string, topics: MemberTopicProgress[]): ActivityItem[] {
    const activities: ActivityItem[] = [];
    const completedTopics = topics.filter(t => t.isCompleted);

    // Create activities based on completed topics
    completedTopics.forEach((topic, index) => {
      if (topic.completedDate) {
        // Started activity
        const startDate = new Date(topic.completedDate);
        startDate.setDate(startDate.getDate() - Math.floor(Math.random() * 7)); // Started 0-7 days before completion

        activities.push({
          date: startDate.toISOString().split('T')[0],
          action: 'Iniciou o estudo do tópico',
          topicTitle: topic.topicTitle,
          type: 'started'
        });

        // Completed activity
        activities.push({
          date: topic.completedDate,
          action: 'Concluiu o tópico',
          topicTitle: topic.topicTitle,
          type: 'completed'
        });
      }
    });

    // Sort by date (most recent first) and return last 10
    return activities
      .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
      .slice(0, 10);
  }
}