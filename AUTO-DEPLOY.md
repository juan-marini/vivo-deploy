# 🚀 DEPLOY AUTOMÁTICO - VIVO ONBOARDING

## ⚡ INSTRUÇÕES RÁPIDAS

### 1️⃣ **FAZER PUSH DO CÓDIGO**
```bash
git add .
git commit -m "feat: configuração para deploy automático"
git push origin main
```

### 2️⃣ **RAILWAY SETUP (Backend + MySQL)**

#### 2.1 Criar conta e projeto:
1. 🔗 Acesse: **https://railway.app/**
2. 🔑 Login com **GitHub**
3. 🆕 Clique **"New Project"**
4. 📂 Selecione **"Deploy from GitHub repo"**
5. 📁 Escolha: **`vivo-onboarding`** (mesmo repo para tudo)
6. ⚙️ Railway usará o **Dockerfile** criado para build automático

#### 2.2 Adicionar MySQL:
1. ➕ No projeto Railway, clique **"+ New"**
2. 🗄️ Selecione **"Database" → "Add MySQL"**
3. ⏳ Aguarde provisionar

#### 2.3 Configurar Backend:
1. 🔧 No serviço do seu repo, vá em **"Variables"**
2. ➕ Adicione estas variáveis (Railway irá fornecer automaticamente do MySQL):
   - `ASPNETCORE_ENVIRONMENT` = `Production`

#### 2.4 Executar SQL:
1. 📋 No MySQL service, clique **"Connect"**
2. 🔗 Use **"MySQL Command Line"**
3. 📄 Copie e cole todo o conteúdo do arquivo **`railway-init.sql`**
4. ▶️ Execute

### 3️⃣ **VERCEL SETUP (Frontend)**

#### 3.1 Obter URL do Backend:
1. ⚙️ No Railway, copie a **URL do seu backend** (ex: `https://backend-production-abc.up.railway.app`)

#### 3.2 Atualizar Frontend:
1. 📝 Edite: `frontend/src/enviroments/enviroment.prod.ts`
2. 🔄 Substitua `BACKEND_URL_PLACEHOLDER` pela URL copiada:
   ```typescript
   export const environment = {
     production: true,
     apiUrl: 'https://SEU-BACKEND-URL-AQUI/api'
   };
   ```

#### 3.3 Deploy no Vercel:
1. 🔗 Acesse: **https://vercel.com/**
2. 🔑 Login com **GitHub**
3. 🆕 Clique **"New Project"**
4. 📁 Importe o **mesmo repo** `vivo-onboarding`
5. ⚙️ Configure:
   - **Root Directory**: `frontend` *(importante!)*
   - **Framework Preset**: `Angular`
   - **Build Command**: `npm run build`
   - **Output Directory**: `dist/frontend`
6. 🚀 Clique **"Deploy"**

---

## ✅ **RESULTADO FINAL**

Após completar os passos:

🌐 **FRONTEND**: `https://seu-projeto.vercel.app`
🔧 **BACKEND**: `https://backend-production-xxx.up.railway.app`
🗄️ **DATABASE**: MySQL no Railway

### 🔐 **LOGIN DE TESTE**
- **Email**: `admin@vivo.com`
- **Senha**: `admin123`

---

## 🛠️ **TROUBLESHOOTING**

### ❌ Backend não inicia:
1. ✅ Verifique se MySQL está conectado no Railway
2. 🔍 Verifique logs no Railway dashboard
3. 🗄️ Confirme se o SQL foi executado corretamente

### ❌ Frontend não carrega dados:
1. 🔗 Confirme URL do backend no `environment.prod.ts`
2. 🌐 Teste a API diretamente: `https://seu-backend/api/auth/test`
3. 🔒 Verifique se CORS está funcionando

### ❌ CORS Error:
- ✅ O backend já tem CORS configurado
- 🔄 Faça um novo deploy se necessário

---

## 📝 **CREDENCIAIS E USUÁRIOS PRÉ-CONFIGURADOS**

| Email | Senha | Perfil |
|-------|-------|--------|
| admin@vivo.com | admin123 | Administrador |
| joao.silva@vivo.com | admin123 | Desenvolvimento |
| maria.santos@vivo.com | admin123 | Desenvolvimento |
| pedro.costa@vivo.com | admin123 | Gestão |
| ana.oliveira@vivo.com | admin123 | Dados |
| carlos.mendes@vivo.com | admin123 | Infraestrutura |

## 🎯 **TEMPO ESTIMADO**: 5-10 minutos

**🚀 Boa sorte com o deploy!**