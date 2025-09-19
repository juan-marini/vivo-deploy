import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { TopicService, Topic, Document, Contact } from '../../core/services/topic.service';
import { AuthService } from '../../core/services/auth.service';
import { FileDownloadService } from '../../core/services/file-download.service';
import { HeaderComponent } from '../../shared/components/header/header.component';

@Component({
  selector: 'app-topic',
  standalone: true,
  imports: [CommonModule, RouterModule, HeaderComponent],
  templateUrl: './topic.component.html',
  styleUrls: ['./topic.component.scss']
})
export class TopicComponent implements OnInit {
  topicId: number = 0;
  topic: Topic | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private topicService: TopicService,
    private authService: AuthService,
    private fileDownloadService: FileDownloadService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.topicId = +params['id'];
      this.loadUserTopics();
    });
  }

  loadUserTopics() {
    // Carregar tópicos específicos do usuário logado
    const currentUser = this.authService.currentUserValue;
    const userId = currentUser?.email || 'default';

    // Carregar tópicos individuais do usuário
    this.topicService.loadUserTopics(userId);

    // Depois carregar o tópico específico
    this.loadTopic();
  }

  loadTopic() {
    this.topicService.getTopicById(this.topicId).subscribe(topic => {
      this.topic = topic;
      if (!this.topic) {
        this.router.navigate(['/home']);
      }
    });
  }

  markAsCompleted() {
    if (this.topic) {
      const currentUser = this.authService.currentUserValue;
      const userId = currentUser?.email || 'default';

      this.topicService.markTopicAsCompleted(this.topic.id, userId);
      this.topic.isCompleted = true;
      console.log('Tópico marcado como concluído:', this.topic.title, 'para usuário:', userId);
    }
  }

  downloadDocument(doc: Document) {
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

  openLink(link: Document) {
    console.log('Abrindo link:', link.title);
    // Em um app real, abriria o link
    window.open(link.url, '_blank');
  }

  contactPerson(contact: Contact) {
    console.log('Entrando em contato com:', contact.name);
    // Em um app real, abriria o cliente de email ou chat
  }

  goBack() {
    this.router.navigate(['/home']);
  }

}