import { Component, type OnInit } from "@angular/core"
import { CommonModule } from "@angular/common"
import { RouterModule, type ActivatedRoute, Router } from "@angular/router"
import { TopicService, Topic, Document } from '../../core/services/topic.service'
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

      // Adicionar links e contatos estáticos baseados no ID do tópico
      topic.links = this.getLinksForTopic(this.topicId);
      topic.contacts = this.getContactsForTopic(this.topicId);

      this.topic = topic;
      console.log('✅ Tópico carregado com links e contatos:', {
        id: topic.id,
        title: topic.title,
        documents: topic.documents?.length || 0,
        links: topic.links?.length || 0,
        contacts: topic.contacts?.length || 0
      });
    })
  }

  private getLinksForTopic(topicId: number): Document[] {
    const linksMap: { [key: number]: Document[] } = {
      1: [ // Fundamentos de SQL
        { id: 11, title: 'W3Schools SQL Tutorial', type: 'link', url: 'https://www.w3schools.com/sql/' },
        { id: 12, title: 'SQLBolt Interactive', type: 'link', url: 'https://sqlbolt.com/' },
        { id: 13, title: 'SQL Server Documentation', type: 'link', url: 'https://docs.microsoft.com/sql/' }
      ],
      2: [ // Oracle Database
        { id: 21, title: 'Oracle Documentation', type: 'link', url: 'https://docs.oracle.com/database/' },
        { id: 22, title: 'Oracle Learning Library', type: 'link', url: 'https://apexapps.oracle.com/pls/apex/r/dbpm/livelabs/home' }
      ],
      3: [ // MongoDB NoSQL
        { id: 31, title: 'MongoDB University', type: 'link', url: 'https://university.mongodb.com/' },
        { id: 32, title: 'MongoDB Documentation', type: 'link', url: 'https://docs.mongodb.com/' }
      ],
      4: [ // Angular Framework
        { id: 41, title: 'Angular Official Docs', type: 'link', url: 'https://angular.io/docs' },
        { id: 42, title: 'Angular CLI Guide', type: 'link', url: 'https://angular.io/cli' }
      ],
      5: [ // React Development
        { id: 51, title: 'React Official Docs', type: 'link', url: 'https://reactjs.org/docs' },
        { id: 52, title: 'React Hooks Guide', type: 'link', url: 'https://reactjs.org/docs/hooks-intro.html' }
      ],
      6: [ // Metodologias Ágeis
        { id: 61, title: 'Scrum Guide', type: 'link', url: 'https://scrumguides.org/' },
        { id: 62, title: 'Agile Manifesto', type: 'link', url: 'https://agilemanifesto.org/' }
      ],
      7: [ // Testes Unitários
        { id: 71, title: 'xUnit Documentation', type: 'link', url: 'https://xunit.net/' },
        { id: 72, title: 'Jest Testing Framework', type: 'link', url: 'https://jestjs.io/' }
      ],
      8: [ // Docker e Containers
        { id: 81, title: 'Docker Documentation', type: 'link', url: 'https://docs.docker.com/' },
        { id: 82, title: 'Docker Hub', type: 'link', url: 'https://hub.docker.com/' }
      ],
      9: [ // Segurança da Informação
        { id: 91, title: 'OWASP Top 10', type: 'link', url: 'https://owasp.org/www-project-top-ten/' },
        { id: 92, title: 'Security Best Practices', type: 'link', url: 'https://cheatsheetseries.owasp.org/' }
      ],
      10: [ // Power BI
        { id: 101, title: 'Power BI Learning', type: 'link', url: 'https://powerbi.microsoft.com/learning/' },
        { id: 102, title: 'Power BI Documentation', type: 'link', url: 'https://docs.microsoft.com/power-bi/' }
      ],
      11: [ // Clean Code
        { id: 111, title: 'Clean Code Principles', type: 'link', url: 'https://clean-code-developer.com/' },
        { id: 112, title: 'Refactoring Guru', type: 'link', url: 'https://refactoring.guru/' }
      ],
      12: [ // Git e Versionamento
        { id: 121, title: 'Git Documentation', type: 'link', url: 'https://git-scm.com/doc' },
        { id: 122, title: 'Atlassian Git Tutorials', type: 'link', url: 'https://www.atlassian.com/git/tutorials' }
      ],
      13: [ // Python para Dados
        { id: 131, title: 'Python Data Science Handbook', type: 'link', url: 'https://jakevdp.github.io/PythonDataScienceHandbook/' },
        { id: 132, title: 'Pandas Documentation', type: 'link', url: 'https://pandas.pydata.org/docs/' }
      ],
      14: [ // Kubernetes
        { id: 141, title: 'Kubernetes Documentation', type: 'link', url: 'https://kubernetes.io/docs/' },
        { id: 142, title: 'Kubernetes Tutorials', type: 'link', url: 'https://kubernetes.io/docs/tutorials/' }
      ],
      15: [ // Liderança e Gestão
        { id: 151, title: 'Harvard Business Review', type: 'link', url: 'https://hbr.org/topic/leadership' },
        { id: 152, title: 'Management 3.0', type: 'link', url: 'https://management30.com/' }
      ],
      16: [ // Machine Learning
        { id: 161, title: 'Google AI Education', type: 'link', url: 'https://ai.google/education/' },
        { id: 162, title: 'Coursera ML Course', type: 'link', url: 'https://www.coursera.org/learn/machine-learning' }
      ],
      17: [ // APIs RESTful
        { id: 171, title: 'REST API Tutorial', type: 'link', url: 'https://restfulapi.net/' },
        { id: 172, title: 'Postman Learning Center', type: 'link', url: 'https://learning.postman.com/' }
      ],
      18: [ // Cybersecurity
        { id: 181, title: 'NIST Cybersecurity Framework', type: 'link', url: 'https://www.nist.gov/cyberframework' },
        { id: 182, title: 'SANS Institute', type: 'link', url: 'https://www.sans.org/' }
      ],
      19: [ // Product Management
        { id: 191, title: 'Product Management Guide', type: 'link', url: 'https://www.productplan.com/learn/' },
        { id: 192, title: 'Mind the Product', type: 'link', url: 'https://www.mindtheproduct.com/' }
      ],
      20: [ // UX/UI Design
        { id: 201, title: 'Nielsen Norman Group', type: 'link', url: 'https://www.nngroup.com/' },
        { id: 202, title: 'Material Design', type: 'link', url: 'https://material.io/design' }
      ]
    };
    return linksMap[topicId] || [];
  }

  private getContactsForTopic(topicId: number): any[] {
    const contactsMap: { [key: number]: any[] } = {
      1: [ // Fundamentos de SQL
        { id: 11, name: 'Maria Database', role: 'DBA Senior', email: 'maria.db@vivo.com.br', phone: '(11) 9999-1001', department: 'Dados' }
      ],
      2: [ // Oracle Database
        { id: 21, name: 'Carlos Oracle', role: 'Especialista Oracle', email: 'carlos.oracle@vivo.com.br', phone: '(11) 9999-2001', department: 'Dados' }
      ],
      3: [ // MongoDB NoSQL
        { id: 31, name: 'Ana NoSQL', role: 'Developer MongoDB', email: 'ana.nosql@vivo.com.br', phone: '(11) 9999-3001', department: 'Desenvolvimento' }
      ],
      4: [ // Angular Framework
        { id: 41, name: 'Pedro Frontend', role: 'Tech Lead Frontend', email: 'pedro.frontend@vivo.com.br', phone: '(11) 9999-4001', department: 'Desenvolvimento' },
        { id: 42, name: 'Julia Angular', role: 'Desenvolvedora Angular', email: 'julia.angular@vivo.com.br', phone: '(11) 9999-4002', department: 'Desenvolvimento' }
      ],
      5: [ // React Development
        { id: 51, name: 'Lucas React', role: 'React Developer', email: 'lucas.react@vivo.com.br', phone: '(11) 9999-5001', department: 'Desenvolvimento' }
      ],
      6: [ // Metodologias Ágeis
        { id: 61, name: 'Roberto Agile', role: 'Agile Coach', email: 'roberto.agile@vivo.com.br', phone: '(11) 9999-6001', department: 'Gestão' },
        { id: 62, name: 'Fernanda Scrum', role: 'Scrum Master', email: 'fernanda.scrum@vivo.com.br', phone: '(11) 9999-6002', department: 'Gestão' }
      ],
      7: [ // Testes Unitários
        { id: 71, name: 'Diana QA', role: 'QA Lead', email: 'diana.qa@vivo.com.br', phone: '(11) 9999-7001', department: 'QA' }
      ],
      8: [ // Docker e Containers
        { id: 81, name: 'Bruno DevOps', role: 'DevOps Engineer', email: 'bruno.devops@vivo.com.br', phone: '(11) 9999-8001', department: 'Infraestrutura' }
      ],
      9: [ // Segurança da Informação
        { id: 91, name: 'Cristina Security', role: 'Security Analyst', email: 'cristina.sec@vivo.com.br', phone: '(11) 9999-9001', department: 'Segurança' }
      ],
      10: [ // Power BI
        { id: 101, name: 'Felipe BI', role: 'BI Developer', email: 'felipe.bi@vivo.com.br', phone: '(11) 9999-1010', department: 'Dados' }
      ],
      11: [ // Clean Code
        { id: 111, name: 'Eduardo Clean', role: 'Senior Developer', email: 'eduardo.clean@vivo.com.br', phone: '(11) 9999-1111', department: 'Desenvolvimento' }
      ],
      12: [ // Git e Versionamento
        { id: 121, name: 'Marina Git', role: 'DevOps Lead', email: 'marina.git@vivo.com.br', phone: '(11) 9999-1212', department: 'Infraestrutura' }
      ],
      13: [ // Python para Dados
        { id: 131, name: 'Rafael Python', role: 'Data Scientist', email: 'rafael.python@vivo.com.br', phone: '(11) 9999-1313', department: 'Dados' }
      ],
      14: [ // Kubernetes
        { id: 141, name: 'Paula K8s', role: 'Cloud Architect', email: 'paula.k8s@vivo.com.br', phone: '(11) 9999-1414', department: 'Infraestrutura' }
      ],
      15: [ // Liderança e Gestão
        { id: 151, name: 'Carlos Manager', role: 'Engineering Manager', email: 'carlos.manager@vivo.com.br', phone: '(11) 9999-1515', department: 'Gestão' }
      ],
      16: [ // Machine Learning
        { id: 161, name: 'Amanda ML', role: 'ML Engineer', email: 'amanda.ml@vivo.com.br', phone: '(11) 9999-1616', department: 'Dados' }
      ],
      17: [ // APIs RESTful
        { id: 171, name: 'Thiago API', role: 'Backend Developer', email: 'thiago.api@vivo.com.br', phone: '(11) 9999-1717', department: 'Desenvolvimento' }
      ],
      18: [ // Cybersecurity
        { id: 181, name: 'Sophia Security', role: 'Cybersecurity Specialist', email: 'sophia.security@vivo.com.br', phone: '(11) 9999-1818', department: 'Segurança' }
      ],
      19: [ // Product Management
        { id: 191, name: 'Daniel Product', role: 'Product Manager', email: 'daniel.product@vivo.com.br', phone: '(11) 9999-1919', department: 'Produto' }
      ],
      20: [ // UX/UI Design
        { id: 201, name: 'Isabella Design', role: 'UX/UI Designer', email: 'isabella.design@vivo.com.br', phone: '(11) 9999-2020', department: 'Design' }
      ]
    };
    return contactsMap[topicId] || [];
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