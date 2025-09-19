const bcrypt = require('bcryptjs');

// Generate hashes
const adminHash = bcrypt.hashSync('Admin@123', 10);
const userHash = bcrypt.hashSync('Senha@123', 10);

console.log('Admin@123 hash:', adminHash);
console.log('Senha@123 hash:', userHash);

console.log('\nSQL Updates:');
console.log(`UPDATE usuarios SET Senha = '${adminHash}' WHERE Email = 'admin@vivo.com';`);
console.log(`UPDATE usuarios SET Senha = '${userHash}' WHERE Email IN ('joao.silva@vivo.com', 'maria.santos@vivo.com', 'pedro.costa@vivo.com', 'ana.oliveira@vivo.com');`);