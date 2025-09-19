const bcrypt = require('bcryptjs');

// Configurar para usar $2a$ explicitamente
const hashAdmin = bcrypt.hashSync('Admin@123', '$2a$12$aaaaaaaaaaaaaaaaaaaaaa');
const hashUser = bcrypt.hashSync('Senha@123', '$2a$12$aaaaaaaaaaaaaaaaaaaaaa');

console.log('Admin@123 ($2a$):', hashAdmin);
console.log('Senha@123 ($2a$):', hashUser);

// Alternativamente, copiar o padrão dos outros usuários
const admin123Hash = '$2a$12$LQv3c1yqBWVHxIR4hQ/hNOMfzKCpXh8Y5nEi6JJDrOQf1K2v4K2yK';
const senha123Hash = '$2a$12$Yk8.W.jXYHxGCiOl5N5HGOGvKg4jQ3YO8b9Hc7Vk6L3M9N1P2R3S4';

console.log('\nSQL para usar hashes compatíveis:');
console.log(`UPDATE usuarios SET Senha = '${senha123Hash}' WHERE Email = 'admin@vivo.com';`);
console.log(`UPDATE usuarios SET Senha = '${senha123Hash}' WHERE Email IN ('joao.silva@vivo.com', 'maria.santos@vivo.com', 'pedro.costa@vivo.com', 'ana.oliveira@vivo.com');`);