import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common"
import { Router, RouterModule } from "@angular/router"
import { FormsModule } from "@angular/forms"
import { DashboardService, ChartData } from '../../core/services/dashboard.service'
import { TeamMember } from '../../core/services/team.service'
import { HeaderComponent } from '../../shared/components/header/header.component'

@Component({
  selector: "app-dashboard",
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, HeaderComponent],
  templateUrl: "./dashboard.component.html",
  styleUrls: ["./dashboard.component.scss"],
})
export class DashboardComponent implements OnInit {
  selectedTeam = ""
  selectedStatus = ""
  selectedStartDate = ""

  teams: string[] = ["Desenvolvimento", "Infraestrutura", "Dados", "Produto", "Design", "QA", "Gestão"]
  statuses: string[] = ["Concluído", "Em andamento", "Não iniciado", "Atrasado"]

  teamMembers: TeamMember[] = []
  chartData: ChartData[] = []

  constructor(
    private dashboardService: DashboardService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadDashboardData();
    // Atualizar dados automaticamente a cada 10 segundos para melhor responsividade
    setInterval(() => {
      this.refreshDashboardDataSilent();
    }, 10000);
  }

  loadDashboardData() {
    // Subscribe to team members and update chart data automatically
    this.dashboardService.getTeamMembers().subscribe(members => {
      this.teamMembers = members;
      // Update chart data whenever team members change
      this.updateChartData();
    });
  }

  private refreshDashboardDataSilent() {
    // Força refresh dos dados do backend
    this.dashboardService.refreshMemberProgress();

    // Recarrega os dados após um pequeno delay
    setTimeout(() => {
      this.loadDashboardData();
    }, 1000);
  }

  manualRefresh() {
    console.log('🔄 Atualizando dados do dashboard manualmente...');

    // Show loading state (optional visual feedback)
    this.showRefreshFeedback();

    // Force refresh from backend with cache clearing
    this.dashboardService.refreshMemberProgress();

    // Force reload all data with delay to ensure backend response
    setTimeout(() => {
      this.loadDashboardData();
      this.updateChartData();
    }, 500);
  }

  private showRefreshFeedback() {
    // Simple visual feedback for user
    const refreshButtons = document.querySelectorAll('[data-refresh-btn]');
    refreshButtons.forEach(btn => {
      const originalText = btn.innerHTML;
      btn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Atualizando...';

      setTimeout(() => {
        btn.innerHTML = originalText;
      }, 2000);
    });
  }

  private updateChartData() {
    this.dashboardService.getChartData().subscribe(chartData => {
      this.chartData = chartData;
    });
  }

  get filteredMembers(): TeamMember[] {
    return this.teamMembers.filter((member) => {
      const matchesTeam = !this.selectedTeam || (member.team && member.team.toLowerCase().includes(this.selectedTeam.toLowerCase()))
      const matchesStatus = !this.selectedStatus || member.status === this.selectedStatus
      const matchesDate = !this.selectedStartDate || member.startDate.includes(this.selectedStartDate)
      return matchesTeam && matchesStatus && matchesDate
    })
  }

  get totalMembers(): number {
    return this.teamMembers.length
  }

  get completedMembers(): number {
    return this.teamMembers.filter((m) => m.status === "Concluído").length
  }

  get inProgressMembers(): number {
    return this.teamMembers.filter((m) => m.status === "Em andamento").length
  }

  get delayedMembers(): number {
    return this.teamMembers.filter((m) => m.status === "Atrasado").length
  }

  getStatusClass(status: string): string {
    switch (status) {
      case "Concluído":
        return "bg-success"
      case "Em andamento":
        return "bg-purple"
      case "Atrasado":
        return "bg-danger"
      default:
        return "bg-secondary"
    }
  }

  getProgressBarClass(status: string): string {
    switch (status) {
      case "Concluído":
        return "bg-success"
      case "Em andamento":
        return "progress-bar-purple"
      case "Atrasado":
        return "bg-danger"
      default:
        return "bg-secondary"
    }
  }

  clearFilters(): void {
    this.selectedTeam = ""
    this.selectedStatus = ""
    this.selectedStartDate = ""
  }

  updateMemberProgress(memberId: string, newProgress: number): void {
    // Método removido - progresso agora vem do backend via tópicos completados
    console.log('Progresso atualizado via backend para:', memberId, newProgress);
    this.dashboardService.refreshMemberProgress();
  }

  // Método removido - membros vêm do BD agora

  simulateProgressUpdate(): void {
    console.log('🔄 Atualizando progresso dos membros...');

    // Show loading state
    this.showRefreshFeedback();

    // Refresh member progress from individual topic completion with force
    this.dashboardService.refreshMemberProgress();

    // Force update chart data and reload all data
    setTimeout(() => {
      this.loadDashboardData();
      this.updateChartData();
    }, 500);

    // Show success message
    setTimeout(() => {
      console.log('✅ Dados atualizados com sucesso!');
    }, 1500);
  }

  openCollaboratorProgress(member: TeamMember): void {
    this.router.navigate(['/collaborator', member.id]);
  }

  resetAllData(): void {
    if (confirm('⚠️ ATENÇÃO: Isso irá resetar o progresso de TODOS os colaboradores e apagar todos os dados de estudo. Esta ação não pode ser desfeita. Deseja continuar?')) {
      this.dashboardService.resetAllMembersData();

      // Force update the dashboard
      setTimeout(() => {
        this.loadDashboardData();
        this.updateChartData();
        alert('✅ Todos os dados foram resetados com sucesso! Todos os colaboradores agora têm 0% de progresso.');
      }, 100);
    }
  }

  getPieChartStyle(): { [key: string]: string } {
    if (!this.chartData || this.chartData.length === 0) {
      return {};
    }

    // Calculate cumulative percentages for the conic gradient
    let cumulativePercentage = 0;
    const gradientStops: string[] = [];

    this.chartData.forEach((item, index) => {
      const startPercentage = cumulativePercentage;
      cumulativePercentage += item.value;
      const endPercentage = cumulativePercentage;

      // Convert percentage to degrees (360deg = 100%)
      const startDegree = (startPercentage * 3.6);
      const endDegree = (endPercentage * 3.6);

      gradientStops.push(`${item.color} ${startDegree}deg ${endDegree}deg`);
    });

    return {
      'background': `conic-gradient(${gradientStops.join(', ')})`
    };
  }

}
