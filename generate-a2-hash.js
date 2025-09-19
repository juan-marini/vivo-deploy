const bcrypt = require('bcryptjs');

// Criar hash $2a$ manualmente
function createBcryptA2Hash(password) {
    // Gerar salt $2a$12$
    const saltRounds = 12;
    const salt = bcrypt.genSaltSync(saltRounds);
    console.log('Salt gerado:', salt);

    // Forçar $2a$ (alguns sistemas usam $2b$ automaticamente)
    const a2Salt = salt.replace('$2b$', '$2a$');
    console.log('Salt $2a$:', a2Salt);

    const hash = bcrypt.hashSync(password, a2Salt);
    console.log(`Hash para "${password}":`, hash);
    return hash;
}

// Gerar hashes
console.log('=== Gerando hashes $2a$ ===');
const adminHash = createBcryptA2Hash('Admin@123');
const userHash = createBcryptA2Hash('Senha@123');

console.log('\n=== SQL Commands ===');
console.log(`-- Admin (Admin@123)`);
console.log(`UPDATE usuarios SET Senha = '${adminHash}' WHERE Email = 'admin@vivo.com';`);
console.log(`-- Colaboradores (Senha@123)`);
console.log(`UPDATE usuarios SET Senha = '${userHash}' WHERE Email IN ('joao.silva@vivo.com', 'maria.santos@vivo.com', 'pedro.costa@vivo.com', 'ana.oliveira@vivo.com');`);