import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';

interface Document {
  id: number;
  title: string;
  type: 'pdf' | 'doc' | 'link';
  url: string;
  size?: string;
}

interface Contact {
  id: number;
  name: string;
  role: string;
  email: string;
  phone: string;
  department: string;
}

interface Topic {
  id: number;
  title: string;
  description: string;
  category: string;
  estimatedTime: string;
  documents: Document[];
  links: Document[];
  contacts: Contact[];
  isCompleted: boolean;
}

@Component({
  selector: 'app-topic',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './topic.component.html',
  styleUrls: ['./topic.component.scss']
})
export class TopicComponent implements OnInit {
  topicId: number = 0;
  topic: Topic | null = null;

  // Mock data - em um app real viria de um service
  topics: Topic[] = [
    {
      id: 1,
      title: 'SQL Server',
      description: 'Banco de dados principal utilizado para armazenar dados de clientes e transações. Aprenda sobre configuração, otimização e melhores práticas.',
      category: 'Banco de Dados',
      estimatedTime: '2h',
      isCompleted: false,
      documents: [
        { id: 1, title: 'Manual SQL Server.pdf', type: 'pdf', url: '#', size: '2.5 MB' },
        { id: 2, title: 'Guia de Consultas.pdf', type: 'pdf', url: '#', size: '1.8 MB' },
        { id: 3, title: 'Configuração de Índices.doc', type: 'doc', url: '#', size: '850 KB' }
      ],
      links: [
        { id: 1, title: 'Portal de Documentação Interna', type: 'link', url: 'https://docs.vivo.com/sql' },
        { id: 2, title: 'Tutorial SQL Server Microsoft', type: 'link', url: 'https://docs.microsoft.com/sql' }
      ],
      contacts: [
        {
          id: 1,
          name: 'Ana Silva',
          role: 'DBA Senior',
          email: 'ana.silva@vivo.com',
          phone: 'Ramal: 1234',
          department: 'Infraestrutura'
        }
      ]
    },
    {
      id: 2,
      title: 'Oracle',
      description: 'Banco de dados secundário utilizado para sistemas específicos e data warehouse.',
      category: 'Banco de Dados',
      estimatedTime: '1.5h',
      isCompleted: false,
      documents: [
        { id: 1, title: 'Oracle Setup Guide.pdf', type: 'pdf', url: '#', size: '3.2 MB' },
        { id: 2, title: 'Query Optimization.pdf', type: 'pdf', url: '#', size: '2.1 MB' }
      ],
      links: [
        { id: 1, title: 'Oracle Documentation', type: 'link', url: 'https://docs.oracle.com' }
      ],
      contacts: [
        {
          id: 1,
          name: 'Carlos Santos',
          role: 'DBA Oracle',
          email: 'carlos.santos@vivo.com',
          phone: 'Ramal: 1567',
          department: 'Infraestrutura'
        }
      ]
    }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.topicId = +params['id'];
      this.loadTopic();
    });
  }

  loadTopic() {
    this.topic = this.topics.find(t => t.id === this.topicId) || null;
    if (!this.topic) {
      this.router.navigate(['/home']);
    }
  }

  markAsCompleted() {
    if (this.topic) {
      this.topic.isCompleted = true;
      // Em um app real, salvaria no backend
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

  onLogout() {
    console.log('Logout clicked');
  }
}