import { Component } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule } from "@angular/router"

interface ProgressItem {
  id: number
  title: string
  completed: boolean
  progress: number
}

@Component({
  selector: "app-progress",
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: "./progress.component.html",
  styleUrls: ["./progress.component.scss"],
})
export class ProgressComponent {
  overallProgress = 60

  progressItems: ProgressItem[] = [
    {
      id: 1,
      title: "SQL Server",
      completed: true,
      progress: 100,
    },
    {
      id: 2,
      title: "Oracle",
      completed: true,
      progress: 100,
    },
    {
      id: 3,
      title: "MongoDB",
      completed: false,
      progress: 0,
    },
    {
      id: 4,
      title: "Ferramentas de Desenvolvimento",
      completed: false,
      progress: 45,
    },
  ]

  get completedItems(): number {
    return this.progressItems.filter((item) => item.completed).length
  }

  get totalItems(): number {
    return this.progressItems.length
  }

  onLogout(): void {
    console.log("Logout clicked")
  }
}
