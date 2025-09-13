import { Component, type OnInit } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule, type ActivatedRoute } from "@angular/router"

interface Document {
  name: string
  type: string
}

interface Link {
  name: string
  url: string
}

interface Contact {
  name: string
  role: string
  email: string
  phone: string
}

interface TopicDetail {
  id: number
  title: string
  description: string
  documents: Document[]
  links: Link[]
  contacts: Contact[]
}

@Component({
  selector: "app-topic-details",
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: "./topic-details.component.html",
  styleUrls: ["./topic-details.component.scss"],
})
export class TopicDetailsComponent implements OnInit {
  topicId = 0
  topic: TopicDetail | null = null

  // Mock data - in real app would come from service
  private topics: TopicDetail[] = [
    {
      id: 1,
      title: "SQL Server",
      description: "Banco de dados principal utilizado para armazenar dados de clientes e transações.",
      documents: [
        { name: "Manual SQL Server.pdf", type: "PDF" },
        { name: "Guia de Consultas.pdf", type: "PDF" },
      ],
      links: [
        { name: "Portal de Documentação Interna", url: "#" },
        { name: "Microsoft SQL Server Docs", url: "#" },
      ],
      contacts: [
        {
          name: "Ana Silva - DBA",
          role: "Database Administrator",
          email: "ana.silva@vivo.com",
          phone: "Ramal: 1234",
        },
      ],
    },
    {
      id: 2,
      title: "Oracle",
      description: "Banco de dados secundário para operações específicas.",
      documents: [{ name: "Oracle Setup Guide.pdf", type: "PDF" }],
      links: [{ name: "Oracle Documentation", url: "#" }],
      contacts: [
        {
          name: "Carlos Santos - DBA",
          role: "Database Administrator",
          email: "carlos.santos@vivo.com",
          phone: "Ramal: 5678",
        },
      ],
    },
  ]

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.topicId = +params["id"]
      this.topic = this.topics.find((t) => t.id === this.topicId) || null
    })
  }

  onMarkCompleted(): void {
    console.log("Marked as completed")
    // In real app, would call service to mark as completed
  }

  onLogout(): void {
    console.log("Logout clicked")
  }
}
