import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { TopicService, Topic, Document, Contact } from '../../core/services/topic.service';
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
    private topicService: TopicService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.topicId = +params['id'];
      this.loadTopic();
    });
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
      this.topicService.markTopicAsCompleted(this.topic.id);
      this.topic.isCompleted = true;
      console.log('Tópico marcado como concluído:', this.topic.title);
    }
  }

  downloadDocument(doc: Document) {
    console.log('Baixando documento:', doc.title);
    // Em um app real, faria o download do documento
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