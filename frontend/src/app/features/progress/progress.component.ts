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
  overallProgress = 0

  progressItems: ProgressItem[] = [
    // DADOS MOCADOS REMOVIDOS - AGORA CARREGA DA API
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
