import { Component } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule } from "@angular/router"
import { FormsModule } from "@angular/forms"

interface TeamMember {
  id: number
  name: string
  role: string
  startDate: string
  progress: number
  status: "Concluído" | "Em andamento" | "Não iniciado" | "Atrasado"
}

interface ChartData {
  label: string
  value: number
  color: string
}

@Component({
  selector: "app-dashboard",
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: "./dashboard.component.html",
  styleUrls: ["./dashboard.component.scss"],
})
export class DashboardComponent {
  selectedTeam = ""
  selectedStatus = ""
  selectedStartDate = ""

  teams: string[] = ["Desenvolvimento", "Infraestrutura", "Dados", "Produto"]
  statuses: string[] = ["Concluído", "Em andamento", "Não iniciado", "Atrasado"]

  teamMembers: TeamMember[] = [
    {
      id: 1,
      name: "João Silva",
      role: "Desenvolvedor Backend",
      startDate: "01/05/2025",
      progress: 70,
      status: "Em andamento",
    },
    {
      id: 2,
      name: "Maria Oliveira",
      role: "Analista de Dados",
      startDate: "15/05/2025",
      progress: 50,
      status: "Em andamento",
    },
    {
      id: 3,
      name: "Carlos Santos",
      role: "Desenvolvedor Frontend",
      startDate: "10/05/2025",
      progress: 30,
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
      progress: 0,
      status: "Não iniciado",
    },
  ]

  chartData: ChartData[] = [
    { label: "Concluído", value: 20, color: "#10B981" },
    { label: "Em andamento", value: 40, color: "#8B5CF6" },
    { label: "Não iniciado", value: 20, color: "#6B7280" },
    { label: "Atrasado", value: 20, color: "#EF4444" },
  ]

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

  onLogout(): void {
    console.log("Logout clicked")
  }
}
