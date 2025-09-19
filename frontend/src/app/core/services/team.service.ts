import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, catchError, of, map } from 'rxjs';
import { environment } from '../../../enviroments/enviroment';

export interface TeamMember {
  id: string;
  name: string;
  role: string;
  startDate: string;
  progress: number;
  status: "Concluído" | "Em andamento" | "Não iniciado" | "Atrasado";
  avatarUrl?: string;
  department?: string;
  phone?: string;
  admissionDate?: Date;
  lastLogin?: Date;
  isActive: boolean;
  team?: string;
}

export interface TeamMemberDto {
  id: string;
  name: string;
  role: string;
  startDate: string;
  progress: number;
  status: string;
  avatarUrl?: string;
  department?: string;
  phone?: string;
  admissionDate?: Date;
  lastLogin?: Date;
  isActive: boolean;
  team: string;
}

@Injectable({
  providedIn: 'root'
})
export class TeamService {
  private apiUrl = `${environment.apiUrl}/progress`;
  private teamMembersSubject = new BehaviorSubject<TeamMember[]>([]);
  public teamMembers$ = this.teamMembersSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadTeamMembers();
  }

  loadTeamMembers(): void {
    this.getTeamMembersFromApi().subscribe({
      next: (members) => {
        this.teamMembersSubject.next(members);
      },
      error: (error) => {
        console.error('Erro ao carregar membros da equipe do backend:', error);
        // Fallback to localStorage or default data if API fails
        this.loadMembersFromStorage();
      }
    });
  }

  private getTeamMembersFromApi(): Observable<TeamMember[]> {
    return this.http.get<TeamMemberDto[]>(`${this.apiUrl}/members`).pipe(
      map(dtos => dtos.map(dto => this.mapDtoToTeamMember(dto))),
      catchError(error => {
        console.error('API Error:', error);
        return of([]);
      })
    );
  }

  private mapDtoToTeamMember(dto: TeamMemberDto): TeamMember {
    return {
      id: dto.id,
      name: dto.name,
      role: dto.role,
      startDate: dto.startDate,
      progress: dto.progress,
      status: this.mapStringToStatus(dto.status),
      avatarUrl: dto.avatarUrl,
      department: dto.department,
      phone: dto.phone,
      admissionDate: dto.admissionDate,
      lastLogin: dto.lastLogin,
      isActive: dto.isActive,
      team: dto.team
    };
  }

  private mapStringToStatus(status: string): TeamMember['status'] {
    switch (status.toLowerCase()) {
      case 'concluído':
      case 'concluido':
        return 'Concluído';
      case 'em andamento':
      case 'em progresso':
        return 'Em andamento';
      case 'não iniciado':
      case 'nao iniciado':
        return 'Não iniciado';
      case 'atrasado':
        return 'Atrasado';
      case 'iniciando':
        return 'Atrasado'; // Map "Iniciando" from backend to "Atrasado" for frontend
      default:
        return 'Não iniciado';
    }
  }

  getTeamMembers(): Observable<TeamMember[]> {
    return this.teamMembers$;
  }

  refreshTeamMembers(): void {
    console.log('Recarregando membros da equipe do backend...');
    // Clear any cached data first
    this.clearCache();
    this.loadTeamMembers();
  }

  forceRefreshTeamMembers(): void {
    console.log('Forçando refresh completo dos dados...');
    // Add timestamp to bypass any HTTP caching
    this.clearCache();
    this.getTeamMembersFromApiWithTimestamp().subscribe({
      next: (members) => {
        this.teamMembersSubject.next(members);
      },
      error: (error) => {
        console.error('Erro ao forçar refresh dos membros:', error);
      }
    });
  }

  private clearCache(): void {
    // Clear localStorage cache if it exists
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        localStorage.removeItem('teamMembers');
      } catch (error) {
        console.log('Could not clear localStorage cache:', error);
      }
    }
  }

  private getTeamMembersFromApiWithTimestamp(): Observable<TeamMember[]> {
    const timestamp = new Date().getTime();
    return this.http.get<TeamMemberDto[]>(`${this.apiUrl}/members?_t=${timestamp}`).pipe(
      map(dtos => dtos.map(dto => this.mapDtoToTeamMember(dto))),
      catchError(error => {
        console.error('API Error with timestamp:', error);
        return of([]);
      })
    );
  }

  private loadMembersFromStorage(): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        const stored = localStorage.getItem('teamMembers');
        if (stored) {
          const members = JSON.parse(stored);
          this.teamMembersSubject.next(members);
        } else {
          // Load default fallback data if nothing in storage
          this.loadDefaultTeamMembers();
        }
      } catch (error) {
        console.error('Error loading team members from storage:', error);
        this.loadDefaultTeamMembers();
      }
    }
  }

  private loadDefaultTeamMembers(): void {
    // No fallback data - always load from backend only
    this.teamMembersSubject.next([]);
  }
}