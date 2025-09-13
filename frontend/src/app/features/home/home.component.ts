import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

interface Topic {
  id: number;
  title: string;
  description: string;
  tags: string[];
  docsCount: number;
  linksCount: number;
  contactsCount: number;
  isCompleted?: boolean;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  searchTerm = '';
  selectedTag = '';

  tags = ['Banco de Dados', 'Ferramentas', 'RH', 'Desenvolvimento', 'Infraestrutura'];

  topics: Topic[] = [
    {
      id: 1,
      title: 'SQL Server',
      description: 'Banco de dados principal da empresa',
      tags: ['Banco de Dados', 'Infraestrutura'],
      docsCount: 3,
      linksCount: 2,
      contactsCount: 1,
      isCompleted: false
    },
    {
      id: 2,
      title: 'Oracle',
      description: 'Banco de dados secundário',
      tags: ['Banco de Dados'],
      docsCount: 2,
      linksCount: 1,
      contactsCount: 1,
      isCompleted: false
    },
    {
      id: 3,
      title: 'MongoDB',
      description: 'Banco de dados NoSQL para projetos específicos',
      tags: ['Banco de Dados', 'Desenvolvimento'],
      docsCount: 4,
      linksCount: 3,
      contactsCount: 2,
      isCompleted: false
    },
    {
      id: 4,
      title: 'Ferramentas de Desenvolvimento',
      description: 'IDEs, frameworks e bibliotecas utilizadas',
      tags: ['Ferramentas', 'Desenvolvimento'],
      docsCount: 8,
      linksCount: 5,
      contactsCount: 3,
      isCompleted: true
    },
    {
      id: 5,
      title: 'Políticas de RH',
      description: 'Diretrizes e procedimentos de recursos humanos',
      tags: ['RH'],
      docsCount: 6,
      linksCount: 2,
      contactsCount: 4,
      isCompleted: false
    }
  ];

  get filteredTopics(): Topic[] {
    return this.topics.filter(topic => {
      const matchesSearch = !this.searchTerm ||
        topic.title.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        topic.description.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesTag = !this.selectedTag || topic.tags.includes(this.selectedTag);

      return matchesSearch && matchesTag;
    });
  }

  clearSearch() {
    this.searchTerm = '';
    this.selectedTag = '';
  }

  onLogout() {
    console.log('Logout clicked');
  }
}