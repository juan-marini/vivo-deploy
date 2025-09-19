import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../enviroments/enviroment';
import { TopicService } from './topic.service';
import { TeamService, TeamMember } from './team.service';

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
  constructor(
    private http: HttpClient,
    private topicService: TopicService,
    private teamService: TeamService
  ) {}

  getTeamMembers(): Observable<TeamMember[]> {
    return this.teamService.getTeamMembers();
  }

  refreshMemberProgress(): void {
    console.log('Atualizando dados dos membros do backend...');
    this.teamService.forceRefreshTeamMembers();
  }

  getChartData(): Observable<ChartData[]> {
    return new Observable(observer => {
      const subscription = this.getTeamMembers().subscribe({
        next: (members) => {
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
            }
          ];

          observer.next(chartData);
          observer.complete();
        },
        error: (error) => {
          observer.error(error);
        }
      });

      return () => subscription.unsubscribe();
    });
  }

  private calculateStatusCounts(members: TeamMember[]) {
    return members.reduce(
      (counts, member) => {
        switch (member.status) {
          case "Concluído":
            counts.completed++;
            break;
          case "Em andamento":
            counts.inProgress++;
            break;
          case "Atrasado":
            counts.delayed++;
            break;
          default:
            counts.notStarted++;
        }
        return counts;
      },
      { completed: 0, inProgress: 0, notStarted: 0, delayed: 0 }
    );
  }

  resetAllMembersData(): void {
    // Call the backend API to reset all progress data
    console.log('Resetting all member data...');
    const apiUrl = `${environment.apiUrl}/progress/fix-database`;
    this.http.post(apiUrl, {}).subscribe({
      next: (response) => {
        console.log('Database reset successful:', response);
        this.refreshMemberProgress();
      },
      error: (error) => {
        console.error('Error resetting database:', error);
        // Still refresh to get latest data
        this.refreshMemberProgress();
      }
    });
  }

  getMemberDetailedProgress(memberId: string): Observable<MemberDetailedProgress> {
    const apiUrl = `${environment.apiUrl}/progress/member/${memberId}`;
    return this.http.get<MemberDetailedProgress>(apiUrl);
  }
}