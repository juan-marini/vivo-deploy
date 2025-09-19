# Vivo Onboarding - Sistema de Treinamento

Sistema completo de onboarding e treinamento para colaboradores da Vivo, desenvolvido com Angular (frontend) e ASP.NET Core (backend), com banco de dados MySQL.

## 📋 Funcionalidades

- **Autenticação JWT**: Login seguro para colaboradores, gestores e administradores
- **Dashboard Personalizado**: Visualização de progresso e métricas por perfil
- **Gestão de Tópicos**: Catálogo de materiais de treinamento com PDFs
- **Acompanhamento de Progresso**: Controle individual e por equipe
- **Relatórios Gerenciais**: Visão completa do progresso das equipes
- **Sistema de Arquivos**: Upload e download de materiais PDF

## 🏗️ Arquitetura

```
vivo-onboarding/
├── frontend/          # Angular 17 application
├── backend/           # ASP.NET Core 9.0 API
├── database/          # Scripts de inicialização MySQL
└── docker-compose.yml # Orquestração completa
```

## 🐳 Executando com Docker (Recomendado)

### Pré-requisitos
- Docker Desktop instalado
- Docker Compose instalado

### 1. Clone o repositório
```bash
git clone <url-do-repositorio>
cd vivo-onboarding
```

### 2. Execute o projeto completo
```bash
# Construir e iniciar todos os serviços
docker-compose up --build

# Ou em background
docker-compose up --build -d
```

### 3. Acesse a aplicação
- **Frontend**: http://localhost
- **Backend API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **MySQL**: localhost:3306

### 4. Credenciais padrão
```
Email: admin@vivo.com
Senha: admin123
```

### Comandos Docker úteis
```bash
# Parar todos os serviços
docker-compose down

# Parar e remover volumes (reset completo)
docker-compose down -v

# Ver logs em tempo real
docker-compose logs -f

# Ver logs de um serviço específico
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f mysql

# Reconstruir apenas um serviço
docker-compose up --build backend

# Executar comandos no container
docker-compose exec backend bash
docker-compose exec mysql mysql -u root -p
```

## 💻 Executando Localmente (Desenvolvimento)

### Pré-requisitos
- Node.js 18+ e npm
- .NET 9.0 SDK
- MySQL Server
- XAMPP (opcional, para MySQL)

### Backend (.NET)
```bash
cd backend

# Restaurar dependências
dotnet restore

# Configurar banco de dados
# 1. Certifique-se que o MySQL está rodando
# 2. Execute o script database/init.sql no MySQL

# Executar a API
dotnet run
```

### Frontend (Angular)
```bash
cd frontend

# Instalar dependências
npm install

# Executar em modo desenvolvimento
npm start

# Aplicação estará em http://localhost:4200
```

## 🗄️ Banco de Dados

### Estrutura Principal
- **Users**: Usuários do sistema (colaboradores, gestores, admins)
- **Teams**: Equipes organizacionais
- **Topics**: Tópicos de treinamento
- **Progress**: Progresso dos usuários nos tópicos

### Configuração Manual
Se não estiver usando Docker:

1. Instale o MySQL
2. Crie o banco `vivo_knowledge_db`
3. Execute o script `database/init.sql`

## 🔧 Configuração

### Variáveis de Ambiente (Docker)
O docker-compose.yml já configura todas as variáveis necessárias.

### Configuração Local
Configure o `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=vivo_knowledge_db;Uid=root;Pwd=;"
  },
  "JwtSettings": {
    "Secret": "MyVerySecretKeyThatIsVeryLongAndSecureForJWTTokenGeneration12345",
    "Issuer": "VivoKnowledgeApi",
    "Audience": "VivoKnowledgeApp",
    "ExpirationInHours": 2
  }
}
```

## 📁 Estrutura de Arquivos

### Frontend
```
frontend/
├── src/
│   ├── app/
│   │   ├── core/          # Serviços, guards, interceptors
│   │   ├── features/      # Componentes de funcionalidades
│   │   ├── shared/        # Componentes compartilhados
│   │   └── models/        # Interfaces e modelos
│   └── assets/            # Recursos estáticos
```

### Backend
```
backend/
├── Controllers/           # API Controllers
├── Data/                 # Entity Framework context
├── Models/               # Entidades e DTOs
├── Services/             # Lógica de negócio
├── Utils/                # Utilitários e helpers
└── wwwroot/files/        # Arquivos PDF estáticos
```

## 👥 Perfis de Usuário

### Colaborador
- Visualizar tópicos disponíveis
- Marcar progresso em tópicos
- Download de materiais PDF
- Dashboard pessoal

### Gestor
- Todas as funcionalidades do colaborador
- Visualizar progresso da equipe
- Relatórios de acompanhamento
- Dashboard gerencial

### Administrador
- Todas as funcionalidades anteriores
- Gestão completa de usuários
- Gestão de tópicos e conteúdo
- Relatórios globais

## 🚀 Deploy

### Produção com Docker
1. Configure as variáveis de ambiente adequadas
2. Use `docker-compose.prod.yml` (se disponível)
3. Configure SSL/HTTPS
4. Configure backup do banco de dados

### Considerações de Segurança
- Altere as senhas padrão
- Configure HTTPS em produção
- Use secrets management para JWT
- Configure firewall adequadamente

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

## 📞 Suporte

Para dúvidas e suporte, entre em contato com a equipe de desenvolvimento.

---

**Desenvolvido com ❤️ para a Vivo**