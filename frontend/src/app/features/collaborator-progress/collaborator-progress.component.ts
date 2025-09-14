import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DashboardService, MemberDetailedProgress } from '../../core/services/dashboard.service';
import { HeaderComponent } from '../../shared/components/header/header.component';

@Component({
  selector: 'app-collaborator-progress',
  standalone: true,
  imports: [CommonModule, HeaderComponent],
  templateUrl: './collaborator-progress.component.html',
  styleUrls: ['./collaborator-progress.component.scss']
})
export class CollaboratorProgressComponent implements OnInit {
  collaboratorId: string | null = null;
  collaboratorData: MemberDetailedProgress | null = null;
  isLoading = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private dashboardService: DashboardService
  ) {}

  ngOnInit() {
    this.collaboratorId = this.route.snapshot.paramMap.get('id');
    if (this.collaboratorId) {
      this.loadCollaboratorData();
    } else {
      this.router.navigate(['/dashboard']);
    }
  }

  private loadCollaboratorData() {
    if (!this.collaboratorId) return;

    this.isLoading = true;

    this.dashboardService.getMemberDetailedProgress(this.collaboratorId).subscribe({
      next: (data) => {
        this.collaboratorData = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar dados do colaborador:', error);
        this.isLoading = false;
        this.router.navigate(['/dashboard']);
      }
    });
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }

  // Métodos para cálculos de progresso
  getOverallProgress(): number {
    if (!this.collaboratorData) return 0;
    return Math.round((this.collaboratorData.completedTopics / this.collaboratorData.totalTopics) * 100);
  }

  getCompletedTopics(): any[] {
    if (!this.collaboratorData) return [];
    return this.collaboratorData.topicsProgress.filter(topic => topic.isCompleted);
  }

  getCurrentTopics(): any[] {
    if (!this.collaboratorData) return [];
    return this.collaboratorData.topicsProgress
      .filter(topic => !topic.isCompleted)
      .slice(0, 2); // Primeiros 2 como "em andamento"
  }

  getPendingTopics(): any[] {
    if (!this.collaboratorData) return [];
    const currentlyStudying = this.getCurrentTopics();
    return this.collaboratorData.topicsProgress
      .filter(topic => !topic.isCompleted)
      .filter(topic => !currentlyStudying.includes(topic));
  }

  getTotalHoursSpent(): string {
    if (!this.collaboratorData) return '0h';
    const completed = this.getCompletedTopics();
    const totalMinutes = completed.reduce((acc, topic) => {
      const time = topic.estimatedTime;
      if (time.includes('h')) {
        return acc + (parseFloat(time) * 60);
      }
      return acc + parseFloat(time);
    }, 0);
    return Math.round(totalMinutes / 60).toString() + 'h';
  }

  getCategoryProgress(): any[] {
    if (!this.collaboratorData) return [];

    const categories: { [key: string]: { total: number; completed: number } } = {};

    this.collaboratorData.topicsProgress.forEach(topic => {
      if (!categories[topic.category]) {
        categories[topic.category] = { total: 0, completed: 0 };
      }
      categories[topic.category].total++;
      if (topic.isCompleted) {
        categories[topic.category].completed++;
      }
    });

    return Object.entries(categories).map(([name, data]) => ({
      name,
      total: data.total,
      completed: data.completed,
      percentage: Math.round((data.completed / data.total) * 100)
    }));
  }

  getCategoryColor(categoryName: string): string {
    const colors: { [key: string]: string } = {
      'Banco de Dados': '#3b82f6',
      'Frontend': '#10b981',
      'Backend': '#8b5cf6',
      'DevOps': '#f59e0b',
      'Análise de Dados': '#ef4444'
    };
    return colors[categoryName] || '#6b7280';
  }

  getProgressMilestones(): any[] {
    const progress = this.getOverallProgress();
    return [
      {
        percentage: 25,
        label: 'Iniciante',
        position: 25,
        icon: 'fas fa-seedling',
        isCompleted: progress >= 25,
        description: 'Primeiros passos no onboarding'
      },
      {
        percentage: 50,
        label: 'Desenvolvendo',
        position: 50,
        icon: 'fas fa-graduation-cap',
        isCompleted: progress >= 50,
        description: 'Aprofundando conhecimentos'
      },
      {
        percentage: 75,
        label: 'Avançado',
        position: 75,
        icon: 'fas fa-rocket',
        isCompleted: progress >= 75,
        description: 'Dominando as tecnologias'
      },
      {
        percentage: 100,
        label: 'Especialista',
        position: 100,
        icon: 'fas fa-crown',
        isCompleted: progress >= 100,
        description: 'Onboarding concluído'
      }
    ];
  }

  getPerformanceInsights(): any[] {
    if (!this.collaboratorData) return [];

    const insights = [];
    const progress = this.getOverallProgress();
    const completedCount = this.getCompletedTopics().length;
    const recentActivity = this.collaboratorData.recentActivity;

    // Insights baseados no progresso
    if (progress >= 80) {
      insights.push({
        type: 'success',
        icon: 'fas fa-trophy',
        title: 'Excelente Desempenho!',
        description: `${this.collaboratorData.member.name} está quase finalizando o onboarding com ${progress}% concluído.`,
        action: 'Considere designar projetos mais desafiadores'
      });
    } else if (progress >= 50) {
      insights.push({
        type: 'info',
        icon: 'fas fa-chart-line',
        title: 'Progresso Consistente',
        description: `Boa evolução com ${completedCount} tópicos concluídos.`,
        action: 'Continue acompanhando o desenvolvimento'
      });
    } else if (progress < 25) {
      insights.push({
        type: 'warning',
        icon: 'fas fa-clock',
        title: 'Início do Processo',
        description: 'Colaborador ainda está se adaptando aos materiais.',
        action: 'Ofereça suporte adicional se necessário'
      });
    }

    // Insights baseados em atividade recente
    if (recentActivity.length === 0) {
      insights.push({
        type: 'alert',
        icon: 'fas fa-pause-circle',
        title: 'Sem Atividade Recente',
        description: 'Nenhuma atividade registrada nos últimos dias.',
        action: 'Verificar se há algum bloqueio ou dificuldade'
      });
    } else if (recentActivity.length >= 3) {
      insights.push({
        type: 'success',
        icon: 'fas fa-fire',
        title: 'Muito Ativo',
        description: 'Alto engajamento com múltiplas atividades recentes.',
        action: 'Colaborador demonstra motivação elevada'
      });
    }

    return insights;
  }

  areAllTopicsCompleted(): boolean {
    if (!this.collaboratorData) return false;
    const allTopics = this.collaboratorData.topicsProgress;
    return allTopics.length > 0 && allTopics.every(topic => topic.isCompleted);
  }

  getNextRecommendations(): any[] {
    if (!this.collaboratorData) return [];

    // Pega todos os tópicos não concluídos (incluindo os "em andamento")
    const notCompleted = this.collaboratorData.topicsProgress
      .filter(topic => !topic.isCompleted)
      .slice(0, 3); // Mostra até 3 próximos tópicos

    return notCompleted.map((topic, index) => ({
      ...topic,
      priority: index === 0 ? 'alta' : index === 1 ? 'média' : 'baixa',
      reason: this.getRecommendationReason(topic, index)
    }));
  }

  private getRecommendationReason(topic: any, index: number): string {
    const reasons = [
      'Próximo tópico prioritário a ser estudado pelo colaborador',
      'Tópico importante para dar continuidade ao aprendizado',
      'Tópico recomendado para completar a formação'
    ];
    return reasons[index] || 'Próximo tópico a ser estudado';
  }

  getActivityIcon(activity: any): string {
    switch (activity.type) {
      case 'completed': return 'fas fa-check-circle';
      case 'started': return 'fas fa-play-circle';
      default: return 'fas fa-info-circle';
    }
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const today = new Date();
    const diffTime = today.getTime() - date.getTime();
    const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays === 0) return 'Hoje';
    if (diffDays === 1) return 'Ontem';
    if (diffDays < 7) return `${diffDays} dias atrás`;
    return date.toLocaleDateString('pt-BR');
  }

  getStatusBadgeClass(): string {
    if (!this.collaboratorData) return 'bg-secondary';

    switch (this.collaboratorData.member.status) {
      case 'Concluído': return 'bg-success';
      case 'Em andamento': return 'bg-primary';
      case 'Atrasado': return 'bg-danger';
      default: return 'bg-secondary';
    }
  }
}