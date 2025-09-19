-- SQL para Aiven MySQL - Vivo Knowledge Database

-- Create Teams table
CREATE TABLE Teams (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create Perfis table
CREATE TABLE perfis (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    Descricao TEXT,
    Ativo BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create usuarios table
CREATE TABLE usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(100) UNIQUE NOT NULL,
    Senha VARCHAR(255) NOT NULL,
    nome_completo VARCHAR(100) NOT NULL,
    perfil_id INT NOT NULL,
    avatar_url VARCHAR(500),
    Ativo BOOLEAN DEFAULT TRUE,
    primeiro_acesso BOOLEAN DEFAULT TRUE,
    data_admissao DATETIME,
    Telefone VARCHAR(20),
    Departamento VARCHAR(100),
    Cargo VARCHAR(100),
    gestor_id INT,
    ultimo_login DATETIME,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (perfil_id) REFERENCES perfis(Id) ON DELETE RESTRICT,
    FOREIGN KEY (gestor_id) REFERENCES usuarios(Id) ON DELETE SET NULL
);

-- Create Users table (for compatibility)
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    Password VARCHAR(255) NOT NULL,
    Role VARCHAR(50) NOT NULL DEFAULT 'Collaborator',
    TeamId INT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (TeamId) REFERENCES Teams(Id) ON DELETE SET NULL
);

-- Create Topics table
CREATE TABLE Topics (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    Content TEXT,
    EstimatedTime VARCHAR(20) DEFAULT '2h',
    PdfFileName VARCHAR(255),
    PdfUrl VARCHAR(500),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create Progress table
CREATE TABLE Progress (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    TopicId INT NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'NotStarted',
    CompletedAt DATETIME NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (TopicId) REFERENCES Topics(Id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_topic (UserId, TopicId)
);

-- Insert initial teams
INSERT INTO Teams (Id, Name, Description) VALUES
(1, 'Desenvolvimento Frontend', 'Equipe responsável pelo desenvolvimento de interfaces e experiência do usuário'),
(2, 'Desenvolvimento Backend', 'Equipe responsável pelo desenvolvimento de APIs e serviços backend'),
(3, 'DevOps e Infraestrutura', 'Equipe responsável pela infraestrutura e deploy das aplicações'),
(4, 'Dados e Analytics', 'Equipe responsável por análise de dados e business intelligence'),
(5, 'Mobile', 'Equipe responsável pelo desenvolvimento de aplicações móveis');

-- Insert initial perfis
INSERT INTO perfis (Id, Nome, Descricao, Ativo) VALUES
(1, 'Administrador', 'Perfil com acesso completo ao sistema', TRUE),
(2, 'Gestão', 'Perfil para gestores de equipe', TRUE),
(3, 'Desenvolvimento', 'Perfil para desenvolvedores', TRUE),
(4, 'Infraestrutura', 'Perfil para equipe de DevOps', TRUE),
(5, 'QA', 'Perfil para analistas de qualidade', TRUE),
(6, 'Produto', 'Perfil para product owners', TRUE),
(7, 'Dados', 'Perfil para analistas de dados', TRUE),
(8, 'Design', 'Perfil para designers', TRUE);

-- Insert initial admin user (password: admin123)
INSERT INTO usuarios (Id, Email, Senha, nome_completo, perfil_id, Ativo, primeiro_acesso) VALUES
(1, 'admin@vivo.com', '$2a$10$X9QGx7NbQGz7GG7UB9/0uOZGZdT9V0gK9VJYxqT5JQZ0Y4Ym7QZ0m', 'Administrador', 1, TRUE, FALSE);

-- Insert sample collaborators
INSERT INTO usuarios (Email, Senha, nome_completo, perfil_id, Ativo, primeiro_acesso, Departamento, Cargo) VALUES
('joao.silva@vivo.com', '$2a$10$X9QGx7NbQGz7GG7UB9/0uOZGZdT9V0gK9VJYxqT5JQZ0Y4Ym7QZ0m', 'João Silva', 3, TRUE, TRUE, 'TI', 'Desenvolvedor Frontend'),
('maria.santos@vivo.com', '$2a$10$X9QGx7NbQGz7GG7UB9/0uOZGZdT9V0gK9VJYxqT5JQZ0Y4Ym7QZ0m', 'Maria Santos', 3, TRUE, TRUE, 'TI', 'Desenvolvedora Backend'),
('pedro.costa@vivo.com', '$2a$10$X9QGx7NbQGz7GG7UB9/0uOZGZdT9V0gK9VJYxqT5JQZ0Y4Ym7QZ0m', 'Pedro Costa', 2, TRUE, FALSE, 'TI', 'Gestor de TI'),
('ana.oliveira@vivo.com', '$2a$10$X9QGx7NbQGz7GG7UB9/0uOZGZdT9V0gK9VJYxqT5JQZ0Y4Ym7QZ0m', 'Ana Oliveira', 7, TRUE, TRUE, 'Dados', 'Analista de Dados');

-- Insert sample users for compatibility
INSERT INTO Users (Name, Email, Password, Role, TeamId) VALUES
('João Silva', 'joao.silva@vivo.com', '$2a$11$rQw8qB3FPzJX8pzK4mYzKe6PzJXrfYcKdW1GzYLnN6YvtQs2pYwK6', 'Collaborator', 1),
('Maria Santos', 'maria.santos@vivo.com', '$2a$11$rQw8qB3FPzJX8pzK4mYzKe6PzJXrfYcKdW1GzYLnN6YvtQs2pYwK6', 'Collaborator', 2),
('Pedro Costa', 'pedro.costa@vivo.com', '$2a$11$rQw8qB3FPzJX8pzK4mYzKe6PzJXrfYcKdW1GzYLnN6YvtQs2pYwK6', 'Manager', 3),
('Ana Oliveira', 'ana.oliveira@vivo.com', '$2a$11$rQw8qB3FPzJX8pzK4mYzKe6PzJXrfYcKdW1GzYLnN6YvtQs2pYwK6', 'Collaborator', 4);

-- Insert initial topics
INSERT INTO Topics (Id, Title, Description, Content, EstimatedTime, PdfFileName, PdfUrl) VALUES
(1, 'Fundamentos de SQL', 'Aprenda os conceitos básicos de SQL e banco de dados relacionais', 'Conteúdo sobre SQL básico...', '2h', 'sql-basics.pdf', '/files/sql-basics.pdf'),
(2, 'Oracle Database', 'Conceitos avançados do Oracle Database', 'Conteúdo sobre Oracle...', '3h', 'oracle-advanced.pdf', '/files/oracle-advanced.pdf'),
(3, 'MongoDB NoSQL', 'Introdução aos bancos de dados NoSQL com MongoDB', 'Conteúdo sobre MongoDB...', '2.5h', 'mongodb-intro.pdf', '/files/mongodb-intro.pdf'),
(4, 'Angular Framework', 'Desenvolvimento frontend com Angular', 'Conteúdo sobre Angular...', '4h', 'angular-guide.pdf', '/files/angular-guide.pdf'),
(5, 'React Development', 'Criação de interfaces com React', 'Conteúdo sobre React...', '3.5h', 'react-tutorial.pdf', '/files/react-tutorial.pdf');

-- Insert initial progress data
INSERT INTO Progress (UserId, TopicId, Status, CompletedAt) VALUES
(2, 1, 'Completed', '2024-01-15 10:30:00'),
(2, 2, 'InProgress', NULL),
(3, 1, 'Completed', '2024-01-10 14:20:00'),
(3, 3, 'Completed', '2024-01-20 16:45:00');

-- Verification query
SELECT 'Database setup completed successfully!' as result;