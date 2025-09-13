import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule } from "@angular/router"
import { FormsModule } from "@angular/forms"
import { DashboardService, TeamMember, ChartData } from '../../core/services/dashboard.service'
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

  constructor(private dashboardService: DashboardService) {}

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    // Subscribe to team members and update chart data automatically
    this.dashboardService.getTeamMembers().subscribe(members => {
      this.teamMembers = members;
      // Update chart data whenever team members change
      this.updateChartData();
    });
  }

  private updateChartData() {
    this.dashboardService.getChartData().subscribe(chartData => {
      this.chartData = chartData;
    });
  }

  get filteredMembers(): TeamMember[] {
    return this.teamMembers.filter((member) => {
      const matchesTeam = !this.selectedTeam || member.role.toLowerCase().includes(this.selectedTeam.toLowerCase())
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
    this.dashboardService.updateMemberProgress(memberId, newProgress);
  }

  // Método removido - membros vêm do BD agora

  simulateProgressUpdate(): void {
    // Refresh member progress from individual topic completion
    this.dashboardService.refreshMemberProgress();

    // Force update chart data immediately
    setTimeout(() => {
      this.updateChartData();
    }, 100);
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
