-- ============================================================
-- Banco de dados: O Garimpo Chic
-- Execute este arquivo uma vez para criar toda a estrutura
-- ============================================================

CREATE DATABASE IF NOT EXISTS garimpo_chic
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE garimpo_chic;

-- ── Usuários ──────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS usuarios (
  id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  cpf         CHAR(11) NOT NULL UNIQUE,
  nome        VARCHAR(100) NOT NULL,
  email       VARCHAR(150) NOT NULL UNIQUE,
  whatsapp    VARCHAR(20),
  senha_hash  VARCHAR(255) NOT NULL,
  ativo       TINYINT(1) NOT NULL DEFAULT 1,
  criado_em   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ── Papéis (roles) ────────────────────────────────────────
CREATE TABLE IF NOT EXISTS papeis (
  id   TINYINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  nome ENUM('CLIENTE','VENDEDORA_PARA_LOJA','FUNCIONARIA','ADMIN') NOT NULL UNIQUE
);

INSERT IGNORE INTO papeis (nome) VALUES
  ('CLIENTE'),
  ('VENDEDORA_PARA_LOJA'),
  ('FUNCIONARIA'),
  ('ADMIN');

-- ── Relação usuário ↔ papéis (N:N) ───────────────────────
CREATE TABLE IF NOT EXISTS usuario_papeis (
  id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  usuario_id  INT UNSIGNED NOT NULL,
  papel_id    TINYINT UNSIGNED NOT NULL,
  ativo       TINYINT(1) NOT NULL DEFAULT 1,
  criado_em   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uq_usuario_papel (usuario_id, papel_id),
  FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE,
  FOREIGN KEY (papel_id)   REFERENCES papeis(id)
);

-- ── Clientes ──────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS clientes (
  id              INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  usuario_id      INT UNSIGNED NOT NULL UNIQUE,
  tamanho_calca   VARCHAR(10),
  tamanho_camisa  VARCHAR(10),
  tamanho_top     VARCHAR(10),
  tamanho_tenis   VARCHAR(6),
  FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- ── Vendedoras para a loja ────────────────────────────────
CREATE TABLE IF NOT EXISTS vendedoras_loja (
  id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  usuario_id  INT UNSIGNED NOT NULL UNIQUE,
  endereco    TEXT,
  status      ENUM('EM_ANALISE','APROVADA','BLOQUEADA') NOT NULL DEFAULT 'EM_ANALISE',
  FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- ── Funcionárias ──────────────────────────────────────────
CREATE TABLE IF NOT EXISTS funcionarias (
  id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  usuario_id  INT UNSIGNED NOT NULL UNIQUE,
  cargo       VARCHAR(60),
  ativa       TINYINT(1) NOT NULL DEFAULT 1,
  FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- ── Roupas ────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS roupas (
  id            INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  vendedora_id  INT UNSIGNED NOT NULL,
  titulo        VARCHAR(120) NOT NULL,
  descricao     TEXT,
  categoria     ENUM('CALCA','TOP','CAMISETA','REGATA','SHORT','TENIS','OUTRO') NOT NULL,
  tamanho       VARCHAR(10) NOT NULL,
  marca         VARCHAR(80),
  estado        ENUM('NOVO','OTIMO','BOM','REGULAR') NOT NULL DEFAULT 'BOM',
  preco         DECIMAL(8,2),
  status        ENUM('EM_AVALIACAO','DISPONIVEL','VENDIDA','RECUSADA') NOT NULL DEFAULT 'EM_AVALIACAO',
  criada_em     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (vendedora_id) REFERENCES vendedoras_loja(id)
);

-- ── Imagens das roupas ────────────────────────────────────
CREATE TABLE IF NOT EXISTS imagens_roupas (
  id         INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  roupa_id   INT UNSIGNED NOT NULL,
  url        TEXT NOT NULL,
  principal  TINYINT(1) NOT NULL DEFAULT 0,
  FOREIGN KEY (roupa_id) REFERENCES roupas(id) ON DELETE CASCADE
);

-- ── Agendamentos de entrega ───────────────────────────────
CREATE TABLE IF NOT EXISTS agendamentos (
  id         INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  roupa_id   INT UNSIGNED NOT NULL UNIQUE,
  data       DATE NOT NULL,
  horario    TIME NOT NULL,
  local      VARCHAR(200),
  status     ENUM('PENDENTE','CONFIRMADO','CONCLUIDO','CANCELADO') NOT NULL DEFAULT 'PENDENTE',
  FOREIGN KEY (roupa_id) REFERENCES roupas(id) ON DELETE CASCADE
);

-- ── Favoritos ─────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS favoritos (
  id             INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  cliente_id     INT UNSIGNED NOT NULL,
  roupa_id       INT UNSIGNED NOT NULL,
  visto_vendido  TINYINT(1) NOT NULL DEFAULT 0,
  criado_em      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uq_favorito (cliente_id, roupa_id),
  FOREIGN KEY (cliente_id) REFERENCES clientes(id) ON DELETE CASCADE,
  FOREIGN KEY (roupa_id)   REFERENCES roupas(id)  ON DELETE CASCADE
);

-- ── Pedidos ───────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS pedidos (
  id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  cliente_id  INT UNSIGNED NOT NULL,
  total       DECIMAL(10,2) NOT NULL DEFAULT 0.00,
  status      ENUM('PENDENTE','PAGO','CANCELADO') NOT NULL DEFAULT 'PENDENTE',
  criado_em   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (cliente_id) REFERENCES clientes(id)
);

-- ── Itens do pedido ───────────────────────────────────────
CREATE TABLE IF NOT EXISTS pedido_itens (
  id         INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  pedido_id  INT UNSIGNED NOT NULL,
  roupa_id   INT UNSIGNED NOT NULL,
  preco      DECIMAL(8,2) NOT NULL,
  FOREIGN KEY (pedido_id) REFERENCES pedidos(id) ON DELETE CASCADE,
  FOREIGN KEY (roupa_id)  REFERENCES roupas(id)
);

-- ============================================================
-- Usuário de teste (senha: 123456)
-- ============================================================
INSERT IGNORE INTO usuarios (cpf, nome, email, senha_hash) VALUES
  ('00000000000', 'Admin Teste', 'admin@garimpo.com',
   '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi');
-- Senha acima = "password" (hash padrão do Laravel/PHP)
-- Troque por: password_hash('suasenha', PASSWORD_DEFAULT)
