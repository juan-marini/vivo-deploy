import { Component, type OnInit } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule, type ActivatedRoute, Router } from "@angular/router"
import { TopicService, Topic } from '../../core/services/topic.service'
import { AuthService } from '../../core/services/auth.service'

@Component({
  selector: "app-topic-details",
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: "./topic-details.component.html",
  styleUrls: ["./topic-details.component.scss"],
})
export class TopicDetailsComponent implements OnInit {
  topicId = 0
  topic: Topic | null = null

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private topicService: TopicService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.topicId = +params["id"]
      this.loadTopic()
    })
  }

  loadTopic(): void {
    this.topicService.getTopicById(this.topicId).subscribe(topic => {
      this.topic = topic
      if (!this.topic) {
        this.router.navigate(['/home'])
      }
    })
  }

  onMarkCompleted(): void {
    if (this.topic) {
      this.topicService.markTopicAsCompleted(this.topic.id)
      this.topic.isCompleted = true
      console.log('Tópico marcado como concluído:', this.topic.title)
    }
  }

  onLogout(): void {
    this.authService.logout()
  }
}