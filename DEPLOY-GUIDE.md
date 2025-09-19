# 🚀 Guia de Deploy - Vivo Onboarding

## 📋 Preparação Concluída
✅ Arquivos de configuração criados
✅ Backend configurado para produção
✅ Frontend configurado para build
✅ Variáveis de ambiente preparadas

---

## 🗄️ Passo 1: Configurar Banco de Dados (PlanetScale)

### 1.1 Criar Conta
1. Acesse: https://planetscale.com/
2. Clique em "Sign up"
3. Use sua conta GitHub/Google

### 1.2 Criar Database
1. Clique em "Create database"
2. Nome: `vivo-knowledge-db`
3. Região: `US East` (mais próximo)
4. Clique em "Create database"

### 1.3 Obter Connection String
1. No dashboard do database criado
2. Clique em "Connect"
3. Selecione "General" ou "Prisma"
4. **COPIE** a connection string (algo como):
   ```
   mysql://username:password@host/database?sslaccept=strict
   ```

---

## 🚂 Passo 2: Deploy Backend (Railway)

### 2.1 Criar Conta
1. Acesse: https://railway.app/
2. Clique em "Login" → "Login with GitHub"
3. Autorize Railway no GitHub

### 2.2 Fazer Deploy
1. Clique em "New Project"
2. Selecione "Deploy from GitHub repo"
3. Escolha seu repositório (faça push primeiro se necessário)
4. Railway detectará automaticamente como .NET

### 2.3 Configurar Variáveis de Ambiente
No painel do Railway:
1. Vá para "Variables"
2. Adicione estas variáveis:

```bash
DATABASE_URL=mysql://sua-connection-string-aqui
JWT_SECRET=MyVerySecretKeyThatIsVeryLongAndSecureForJWTTokenGeneration12345
ASPNETCORE_ENVIRONMENT=Production
```

### 2.4 Obter URL do Backend
1. Após o deploy, copie a URL gerada (ex: `https://backend-production-abc.up.railway.app`)

---

## 🌐 Passo 3: Deploy Frontend (Vercel)

### 3.1 Atualizar URL do Backend
1. Edite o arquivo: `frontend/src/enviroments/enviroment.prod.ts`
2. Substitua `BACKEND_URL_PLACEHOLDER` pela URL do Railway:
   ```typescript
   export const environment = {
     production: true,
     apiUrl: 'https://backend-production-abc.up.railway.app/api'
   };
   ```

### 3.2 Fazer Deploy
1. Acesse: https://vercel.com/
2. Login com GitHub
3. Clique em "New Project"
4. Importe seu repositório
5. Configure:
   - **Root Directory**: `frontend`
   - **Framework**: Angular
   - **Build Command**: `npm run build`
   - **Output Directory**: `dist/frontend`

---

## 🔧 Comandos Necessários

### Push para GitHub (se ainda não fez):
```bash
git add .
git commit -m "feat: configuração para deploy em produção"
git push origin main
```

### Build local para testar (opcional):
```bash
# Backend
cd backend
dotnet build -c Release

# Frontend
cd frontend
npm run build
```

---

## ✅ Checklist Final

### Banco de Dados
- [ ] PlanetScale database criado
- [ ] Connection string copiada
- [ ] Database configurado

### Backend
- [ ] Railway project criado
- [ ] Variáveis de ambiente configuradas
- [ ] Deploy realizado com sucesso
- [ ] URL do backend anotada

### Frontend
- [ ] URL do backend atualizada no environment.prod.ts
- [ ] Vercel project criado
- [ ] Deploy realizado com sucesso
- [ ] Aplicação acessível via URL

---

## 🚨 Troubleshooting

### Se o backend não iniciar:
1. Verifique os logs no Railway
2. Confirme se DATABASE_URL está correto
3. Teste a conexão MySQL no PlanetScale

### Se o frontend não carregar:
1. Verifique se a URL do backend está correta
2. Confirme se o CORS está configurado no backend
3. Teste as APIs no navegador

### Problemas de CORS:
O backend já tem CORS configurado. Se houver problemas:
1. Confirme se a URL frontend está em produção
2. Verifique logs do backend

---

## 🎯 URLs Finais

Após completar todos os passos:

- **Frontend**: `https://seu-app.vercel.app`
- **Backend**: `https://backend-production-abc.up.railway.app`
- **Database**: Gerenciado pelo PlanetScale

---

## 💡 Dicas

1. **Custos**: Todas as plataformas têm planos gratuitos suficientes para começar
2. **SSL**: Automático em todas as plataformas
3. **Domínio personalizado**: Pode ser adicionado depois no Vercel
4. **Monitoramento**: Railway e Vercel fornecem logs em tempo real

**🎉 Boa sorte com o deploy!**