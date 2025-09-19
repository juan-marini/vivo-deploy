import { Component, type OnInit } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule, type ActivatedRoute, Router } from "@angular/router"
import { TopicService, Topic, Document, Contact } from '../../core/services/topic.service'
import { AuthService } from '../../core/services/auth.service'
import { FileDownloadService } from '../../core/services/file-download.service'

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
    private authService: AuthService,
    private fileDownloadService: FileDownloadService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.topicId = +params["id"]
      this.loadTopic()
    })
  }

  loadTopic(): void {
    console.log('🔍 Carregando tópico ID:', this.topicId);
    this.topicService.getTopicById(this.topicId).subscribe(topic => {
      console.log('📄 Tópico recebido:', topic);
      if (!topic) {
        console.log('❌ Tópico não encontrado, redirecionando...');
        this.router.navigate(['/home'])
        return;
      }

      // O backend agora retorna os dados completos com links e contatos
      this.topic = topic;
      console.log('✅ Tópico carregado com links e contatos:', {
        id: topic.id,
        title: topic.title,
        documents: topic.documents?.length || 0,
        links: topic.links?.length || 0,
        contacts: topic.contacts?.length || 0,
        topicCompleto: this.topic
      });
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

  downloadDocument(doc: Document): void {
    console.log('🔽 Download iniciado:', doc.title);
    console.log('📄 Documento:', doc);

    // Mapear títulos para nomes de arquivos reais
    let fileName = '';

    if (doc.title.includes('.NET Core') || doc.title.includes('Guia .NET Core')) {
      fileName = 'dotnet-core-documentation.pdf';
    } else if (doc.title.includes('ASP.NET')) {
      fileName = 'aspnet-core-tutorial.pdf';
    } else {
      fileName = 'exemplo-documento.pdf';
    }

    console.log('📎 Arquivo a ser baixado:', fileName);

    // Usar o serviço que força download real
    this.fileDownloadService.downloadFileDirectly(fileName);
  }
}