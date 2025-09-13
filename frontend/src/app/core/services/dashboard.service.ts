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
    this.updateProgressForAllMembers();
  }

  getTeamMembers(): Observable<TeamMember[]> {
    return this.teamMembers$;
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

  private clearAllUserTopics(): void {
    const userIds = [
      "joao.silva@vivo.com.br",
      "maria.oliveira@vivo.com.br",
      "carlos.santos@vivo.com.br"
    ];

    if (typeof window !== 'undefined' && window.localStorage) {
      userIds.forEach(userId => {
        localStorage.removeItem(`topics_${userId}`);
      });
    }
  }
}