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
    this.dashboardService.getTeamMembers().subscribe(members => {
      this.teamMembers = members;
    });

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

  updateMemberProgress(memberId: number, newProgress: number): void {
    this.dashboardService.updateMemberProgress(memberId, newProgress);
  }

  addNewMember(): void {
    const newMember = {
      name: "Novo Membro",
      role: "Função",
      startDate: new Date().toLocaleDateString('pt-BR'),
      progress: 0,
      status: "Não iniciado" as const
    };
    this.dashboardService.addTeamMember(newMember);
  }

  simulateProgressUpdate(): void {
    // Simular atualização de progresso para demonstração
    const randomMember = this.teamMembers[Math.floor(Math.random() * this.teamMembers.length)];
    if (randomMember && randomMember.progress < 100) {
      const newProgress = Math.min(100, randomMember.progress + Math.floor(Math.random() * 20) + 5);
      this.updateMemberProgress(randomMember.id, newProgress);
    }
  }

}
