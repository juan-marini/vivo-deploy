import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TopicService, Topic } from '../../core/services/topic.service';
import { ProgressService } from '../../core/services/progress.service';
import { AuthService } from '../../core/services/auth.service';
import { HeaderComponent } from '../../shared/components/header/header.component';

interface TopicDisplay {
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
  imports: [CommonModule, RouterModule, FormsModule, HeaderComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  searchTerm = '';
  selectedTag = '';

  tags: string[] = [];
  topics: TopicDisplay[] = [];
  progressStats = {
    completed: 0,
    total: 0,
    percentage: 0
  };

  constructor(
    private topicService: TopicService,
    private progressService: ProgressService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadUserTopics();
    this.loadProgressStats();
    this.loadAvailableCategories();
  }

  loadUserTopics() {
    // Carregar tópicos específicos do usuário logado
    const currentUser = this.authService.currentUserValue;
    const userId = currentUser?.email || 'default';

    // Carregar tópicos individuais do usuário
    this.topicService.loadUserTopics(userId);

    // Depois carregar todos os tópicos
    this.loadTopics();
  }

  loadProgressStats() {
    this.progressService.getProgressStats().subscribe(stats => {
      this.progressStats = stats;
    });
  }

  loadTopics() {
    this.topicService.getTopics().subscribe(topicsData => {
      this.topics = topicsData.map(topic => ({
        id: topic.id,
        title: topic.title,
        description: topic.description,
        tags: [topic.category],
        docsCount: topic.documents.length,
        linksCount: topic.links.length,
        contactsCount: topic.contacts.length,
        isCompleted: topic.isCompleted
      }));
    });
  }

  get filteredTopics(): TopicDisplay[] {
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

  loadAvailableCategories() {
    this.topicService.getAvailableCategories().subscribe(categories => {
      this.tags = categories;
    });
  }

  filterByCategory(category: string) {
    this.selectedTag = this.selectedTag === category ? '' : category;
  }

  getUserAreaTopics() {
    const currentUser = this.authService.currentUserValue;
    if (!currentUser?.perfil) return this.filteredTopics;

    // Mapear perfis para categorias correspondentes
    const areaMapping: { [key: string]: string } = {
      'Desenvolvimento': 'Desenvolvimento',
      'Infraestrutura': 'Infraestrutura',
      'QA': 'QA',
      'Produto': 'Produto',
      'Dados': 'Dados',
      'Design': 'Design',
      'Gestão': 'Gestão'
    };

    const userArea = areaMapping[currentUser.perfil];
    if (!userArea) return this.filteredTopics;

    // Retornar tópicos da área do usuário prioritariamente
    const areaTopics = this.filteredTopics.filter(topic =>
      topic.tags.includes(userArea)
    );

    const otherTopics = this.filteredTopics.filter(topic =>
      !topic.tags.includes(userArea)
    );

    return [...areaTopics, ...otherTopics];
  }

}