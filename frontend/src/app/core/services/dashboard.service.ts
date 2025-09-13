import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { TopicService } from './topic.service';

export interface TeamMember {
  id: number;
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
      id: 1,
      name: "João Silva",
      role: "Desenvolvedor Backend",
      startDate: "01/05/2025",
      progress: 0,
      status: "Não iniciado",
    },
    {
      id: 2,
      name: "Maria Oliveira",
      role: "Analista de Dados",
      startDate: "15/05/2025",
      progress: 83,
      status: "Em andamento",
    },
    {
      id: 3,
      name: "Carlos Santos",
      role: "Desenvolvedor Frontend",
      startDate: "10/05/2025",
      progress: 33,
      status: "Atrasado",
    },
    {
      id: 4,
      name: "Ana Costa",
      role: "DevOps Engineer",
      startDate: "20/04/2025",
      progress: 100,
      status: "Concluído",
    },
    {
      id: 5,
      name: "Pedro Almeida",
      role: "Product Manager",
      startDate: "25/05/2025",
      progress: 67,
      status: "Em andamento",
    },
    {
      id: 6,
      name: "Lucia Fernandes",
      role: "Designer UX/UI",
      startDate: "03/05/2025",
      progress: 50,
      status: "Em andamento",
    },
    {
      id: 7,
      name: "Roberto Sousa",
      role: "Analista de Infraestrutura",
      startDate: "12/05/2025",
      progress: 17,
      status: "Atrasado",
    },
    {
      id: 8,
      name: "Fernanda Lima",
      role: "QA Engineer",
      startDate: "08/05/2025",
      progress: 100,
      status: "Concluído",
    },
    {
      id: 9,
      name: "Marcos Pereira",
      role: "Scrum Master",
      startDate: "28/04/2025",
      progress: 83,
      status: "Em andamento",
    },
    {
      id: 10,
      name: "Camila Torres",
      role: "Desenvolvedora Full Stack",
      startDate: "06/05/2025",
      progress: 0,
      status: "Não iniciado",
    },
  ]);

  public teamMembers$ = this.teamMembersSubject.asObservable();

  constructor(private topicService: TopicService) {
    this.loadMembersFromStorage();
    this.updateProgressBasedOnTopics();
  }

  getTeamMembers(): Observable<TeamMember[]> {
    return this.teamMembers$;
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

  updateMemberProgress(memberId: number, newProgress: number): void {
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

  addTeamMember(member: Omit<TeamMember, 'id'>): void {
    const currentMembers = this.teamMembersSubject.value;
    const newId = Math.max(...currentMembers.map(m => m.id), 0) + 1;
    const newMember: TeamMember = { ...member, id: newId };

    const updatedMembers = [...currentMembers, newMember];
    this.teamMembersSubject.next(updatedMembers);
    this.saveMembersToStorage(updatedMembers);
  }

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

  private updateProgressBasedOnTopics(): void {
    this.topicService.topics$.subscribe(topics => {
      const completionStats = this.topicService.getCompletionStats();

      const currentMembers = this.teamMembersSubject.value;
      const currentUserMember = currentMembers.find(m => m.name === "João Silva");

      if (currentUserMember && currentUserMember.progress !== completionStats.percentage) {
        this.updateMemberProgress(currentUserMember.id, completionStats.percentage);
      }

      // Simular progresso dinâmico para outros membros baseado no tempo
      this.simulateTeamProgress();
    });
  }

  private simulateTeamProgress(): void {
    const currentMembers = this.teamMembersSubject.value;
    const now = new Date();

    const updatedMembers = currentMembers.map(member => {
      if (member.name === "João Silva") {
        return member; // João Silva é controlado pelo progresso real dos tópicos
      }

      // Simular progresso baseado no tempo desde a data de início
      const startDate = new Date(member.startDate.split('/').reverse().join('-'));
      const daysSinceStart = Math.floor((now.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24));

      // Progresso base + variação aleatória
      let newProgress = Math.min(100, Math.max(0, daysSinceStart * 8 + Math.random() * 20));

      // Manter alguns membros com progresso específico para demonstração
      if (member.id === 4 || member.id === 8) newProgress = 100; // Mantém concluídos
      if (member.id === 10) newProgress = Math.min(15, newProgress); // Mantém iniciante

      const newStatus = this.calculateStatusFromProgress(newProgress);

      return {
        ...member,
        progress: Math.round(newProgress),
        status: newStatus
      };
    });

    this.teamMembersSubject.next(updatedMembers);
    this.saveMembersToStorage(updatedMembers);
  }

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
}