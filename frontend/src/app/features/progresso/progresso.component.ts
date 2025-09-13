import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface ProgressItem {
  id: number;
  title: string;
  description: string;
  completed: boolean;
  completedDate?: string;
  category: string;
  estimatedTime: string;
}

@Component({
  selector: 'app-progresso',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './progresso.component.html',
  styleUrls: ['./progresso.component.scss']
})
export class ProgressoComponent {
  progressItems: ProgressItem[] = [
    {
      id: 1,
      title: 'SQL Server',
      description: 'Banco de dados principal da empresa',
      completed: true,
      completedDate: '2025-05-15',
      category: 'Banco de Dados',
      estimatedTime: '2h'
    },
    {
      id: 2,
      title: 'Oracle',
      description: 'Banco de dados secundário',
      completed: true,
      completedDate: '2025-05-18',
      category: 'Banco de Dados',
      estimatedTime: '1.5h'
    },
    {
      id: 3,
      title: 'MongoDB',
      description: 'Banco de dados NoSQL para projetos específicos',
      completed: false,
      category: 'Banco de Dados',
      estimatedTime: '3h'
    },
    {
      id: 4,
      title: 'Ferramentas de Desenvolvimento',
      description: 'IDEs, frameworks e bibliotecas utilizadas',
      completed: true,
      completedDate: '2025-05-12',
      category: 'Desenvolvimento',
      estimatedTime: '4h'
    },
    {
      id: 5,
      title: 'Políticas de RH',
      description: 'Diretrizes e procedimentos de recursos humanos',
      completed: false,
      category: 'RH',
      estimatedTime: '1h'
    },
    {
      id: 6,
      title: 'Infraestrutura AWS',
      description: 'Serviços e configurações da Amazon Web Services',
      completed: false,
      category: 'Infraestrutura',
      estimatedTime: '5h'
    }
  ];

  get totalItems(): number {
    return this.progressItems.length;
  }

  get completedItems(): number {
    return this.progressItems.filter(item => item.completed).length;
  }

  get progressPercentage(): number {
    return Math.round((this.completedItems / this.totalItems) * 100);
  }

  get completedItemsList(): ProgressItem[] {
    return this.progressItems.filter(item => item.completed);
  }

  get pendingItemsList(): ProgressItem[] {
    return this.progressItems.filter(item => !item.completed);
  }

  get estimatedTimeRemaining(): string {
    const pendingItems = this.pendingItemsList;
    let totalMinutes = 0;

    pendingItems.forEach(item => {
      const time = item.estimatedTime;
      if (time.includes('h')) {
        const hours = parseFloat(time.replace('h', ''));
        totalMinutes += hours * 60;
      }
    });

    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    if (hours > 0) {
      return minutes > 0 ? `${hours}h ${minutes}min` : `${hours}h`;
    }
    return `${minutes}min`;
  }

  onLogout() {
    console.log('Logout clicked');
  }
}