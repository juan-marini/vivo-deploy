# 🐳 DEPLOY COM DOCKER - RENDER + VERCEL

## ⚡ ESTRATÉGIA HÍBRIDA PERFEITA!

### 📋 **Arquitetura:**
- 🐳 **Backend**: Docker no Render
- 🌐 **Frontend**: Angular no Vercel (SEM Docker)
- 🗄️ **Database**: PostgreSQL no Render

---

## 🚀 **PASSO 1: PUSH DO CÓDIGO**
```bash
git add .
git commit -m "feat: Docker para backend + deploy híbrido"
git push origin main
```

---

## 🐳 **PASSO 2: BACKEND (Render com Docker)**

### 2.1 Criar conta e serviço:
1. 🔗 **Acesse**: https://render.com/
2. 🔑 **Sign up** com GitHub
3. 🆕 **New** → **Web Service**
4. 📂 **Connect** repositório `vivo-onboarding`

### 2.2 Configurar Docker:
1. ⚙️ **Environment**: `Docker`
2. 📁 **Root Directory**: deixe vazio (usa a raiz)
3. 🐳 **Dockerfile Path**: `./Dockerfile`
4. 🌐 **Port**: `8080`
5. 🚀 **Create Web Service**

### 2.3 Criar banco PostgreSQL:
1. 🆕 **New** → **PostgreSQL**
2. 📝 **Name**: `vivo-knowledge-db`
3. 🚀 **Create Database**

### 2.4 Conectar banco ao backend:
1. No **Web Service**, vá para **Environment**
2. ➕ **Add Environment Variable**:
   - **Key**: `DATABASE_URL`
   - **Value**: Copie da aba **Connect** do PostgreSQL
3. 🔄 **Manual Deploy** para aplicar mudanças

### 2.5 Executar SQL inicial:
1. No **PostgreSQL**, clique **Connect**
2. 📱 Use **External Connection** ou **Shell**
3. 📄 Execute o conteúdo do arquivo `railway-init.sql`

---

## 🌐 **PASSO 3: FRONTEND (Vercel - SEM Docker)**

### 3.1 Obter URL do backend:
1. ✅ Aguarde backend terminar deploy
2. 📋 Copie a URL (ex: `https://vivo-knowledge-api.onrender.com`)

### 3.2 Configurar environment:
1. 📝 Edite `frontend/src/enviroments/enviroment.prod.ts`:
   ```typescript
   export const environment = {
     production: true,
     apiUrl: 'https://SUA-URL-RENDER-AQUI/api'
   };
   ```
2. 💾 **Commit + Push**

### 3.3 Deploy no Vercel:
1. 🔗 **Acesse**: https://vercel.com/
2. 🔑 **Login** com GitHub
3. 🆕 **New Project**
4. 📂 **Import** `vivo-onboarding`
5. ⚙️ **Configure**:
   - **Framework**: `Angular`
   - **Root Directory**: `frontend`
   - **Build Command**: `npm run build`
   - **Output Directory**: `dist/frontend`
6. 🚀 **Deploy**

---

## ✅ **RESULTADO FINAL**

### 🌐 **URLs:**
- **Frontend**: `https://seu-projeto.vercel.app`
- **Backend**: `https://vivo-knowledge-api.onrender.com`
- **API Docs**: `https://vivo-knowledge-api.onrender.com/swagger`

### 🔐 **Login de teste:**
- **Email**: `admin@vivo.com`
- **Senha**: `admin123`

---

## 🎯 **VANTAGENS DESTA ESTRATÉGIA:**

| Componente | Plataforma | Vantagem |
|------------|------------|----------|
| Backend | Render + Docker | ✅ Controle total do ambiente |
| Frontend | Vercel | ✅ CDN global + build otimizado |
| Database | Render PostgreSQL | ✅ Backup automático |

---

## 🛠️ **TROUBLESHOOTING**

### ❌ Docker build falha:
- ✅ Verifique se `Dockerfile` está na raiz
- 🔍 Check logs no Render dashboard
- 📁 Confirme estrutura: `/backend/backend.csproj`

### ❌ Database connection falha:
- 🔗 Use a connection string EXATA do Render
- 🧪 Teste a variável `DATABASE_URL`
- 🔄 Redeploy após configurar variáveis

### ❌ CORS errors:
- ✅ Backend já tem CORS configurado
- 🔄 Força refresh do navegador
- 🌐 Verifique URL no `environment.prod.ts`

---

## 📊 **MONITORAMENTO**

- 📈 **Render**: Logs em tempo real + métricas
- 📊 **Vercel**: Analytics + performance insights
- 🗄️ **PostgreSQL**: Backups automáticos

**🎉 Deploy híbrido = Melhor dos dois mundos!**