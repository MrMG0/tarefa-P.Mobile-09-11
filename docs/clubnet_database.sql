-- ====================================================================
-- SISTEMA DE GESTÃO DE CLUBES - CLUBNET
-- SCRIPT DE BANCO DE DADOS (MYSQL 8.0+)
-- Arquitetura e Modelagem conforme Seção 5 da Especificação
-- ====================================================================

CREATE DATABASE IF NOT EXISTS `clubnet_db` 
  DEFAULT CHARACTER SET utf8mb4 
  DEFAULT COLLATE utf8mb4_unicode_ci;

USE `clubnet_db`;

-- 1. Tabela: clubes (Seção 5.1)
CREATE TABLE IF NOT EXISTS `clubes` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `nome` VARCHAR(200) NOT NULL,
  `sigla` VARCHAR(30) NULL,
  `endereco` VARCHAR(200) NULL,
  `bairro` VARCHAR(100) NULL,
  `cidade` VARCHAR(100) NULL,
  `uf` CHAR(2) NULL,
  `telefone` VARCHAR(20) NULL,
  `email` VARCHAR(200) NULL,
  `ativo` TINYINT(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. Tabela: usuarios (Seção 5.2)
CREATE TABLE IF NOT EXISTS `usuarios` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `nome` VARCHAR(200) NOT NULL,
  `email` VARCHAR(200) NOT NULL,
  `senha_hash` VARCHAR(255) NOT NULL,
  `perfil` VARCHAR(30) NOT NULL COMMENT 'SUPERADMIN, PRESIDENTE, ADMIN, TESOUREIRO, ASSOCIADO, DEPENDENTE',
  `id_clube` INT NULL,
  `ativo` TINYINT(1) NOT NULL DEFAULT 1,
  `data_cadastro` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_usuarios_email` (`email`),
  KEY `fk_usuarios_clube` (`id_clube`),
  CONSTRAINT `fk_usuarios_clube` FOREIGN KEY (`id_clube`) REFERENCES `clubes` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. Tabela: associados (Seção 5.3)
CREATE TABLE IF NOT EXISTS `associados` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_clube` INT NOT NULL,
  `id_usuario` INT NULL,
  `nome` VARCHAR(200) NOT NULL,
  `cpf` VARCHAR(14) NOT NULL,
  `endereco` VARCHAR(200) NULL,
  `bairro` VARCHAR(100) NULL,
  `cidade` VARCHAR(100) NULL,
  `uf` CHAR(2) NULL,
  `sexo` VARCHAR(30) NULL,
  `estado_civil` VARCHAR(30) NULL,
  `telefone` VARCHAR(20) NULL,
  `email` VARCHAR(200) NULL,
  `data_nascimento` DATE NULL,
  `tipo_associado` VARCHAR(30) NOT NULL DEFAULT 'ASSOCIADO' COMMENT 'ASSOCIADO_FUNDADOR, ASSOCIADO, CONVIDADO',
  `situacao_cadastro` VARCHAR(30) NOT NULL DEFAULT 'PENDENTE' COMMENT 'PENDENTE, APROVADO, REJEITADO, INATIVO',
  `data_cadastro` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `fk_associados_clube` (`id_clube`),
  KEY `fk_associados_usuario` (`id_usuario`),
  CONSTRAINT `fk_associados_clube` FOREIGN KEY (`id_clube`) REFERENCES `clubes` (`id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_associados_usuario` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 4. Tabela: dependentes (Seção 5.4)
CREATE TABLE IF NOT EXISTS `dependentes` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_associado` INT NOT NULL,
  `nome` VARCHAR(200) NOT NULL,
  `cpf` VARCHAR(14) NULL,
  `data_nascimento` DATE NULL,
  `parentesco` VARCHAR(50) NULL,
  `telefone` VARCHAR(20) NULL,
  `email` VARCHAR(200) NULL,
  `ativo` TINYINT(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`),
  KEY `fk_dependentes_associado` (`id_associado`),
  CONSTRAINT `fk_dependentes_associado` FOREIGN KEY (`id_associado`) REFERENCES `associados` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 5. Tabela: interesses_associado (Seção 5.5)
CREATE TABLE IF NOT EXISTS `interesses_associado` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_associado` INT NOT NULL,
  `interesse` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_interesses_associado` (`id_associado`),
  CONSTRAINT `fk_interesses_associado` FOREIGN KEY (`id_associado`) REFERENCES `associados` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 6. Tabela: eventos (Seção 5.6)
CREATE TABLE IF NOT EXISTS `eventos` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_clube` INT NOT NULL,
  `titulo` VARCHAR(200) NOT NULL,
  `descricao` TEXT NULL,
  `data_inicio` DATETIME NOT NULL,
  `data_fim` DATETIME NULL,
  `publico_alvo` VARCHAR(30) NOT NULL DEFAULT 'ASSOCIADOS' COMMENT 'ASSOCIADOS ou PUBLICO_GERAL',
  `local` VARCHAR(200) NULL,
  `ativo` TINYINT(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`),
  KEY `fk_eventos_clube` (`id_clube`),
  CONSTRAINT `fk_eventos_clube` FOREIGN KEY (`id_clube`) REFERENCES `clubes` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 7. Tabela: mensalidades (Seção 5.7)
CREATE TABLE IF NOT EXISTS `mensalidades` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_clube` INT NOT NULL,
  `id_associado` INT NOT NULL,
  `referencia_mes` VARCHAR(7) NOT NULL COMMENT 'ex: 2026-11',
  `valor` DECIMAL(10,2) NOT NULL,
  `data_vencimento` DATE NOT NULL,
  `data_pagamento` DATE NULL,
  `situacao` VARCHAR(20) NOT NULL DEFAULT 'ABERTA' COMMENT 'ABERTA, PAGA, VENCIDA, CANCELADA',
  `observacao` TEXT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_mensalidades_clube` (`id_clube`),
  KEY `fk_mensalidades_associado` (`id_associado`),
  CONSTRAINT `fk_mensalidades_clube` FOREIGN KEY (`id_clube`) REFERENCES `clubes` (`id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_mensalidades_associado` FOREIGN KEY (`id_associado`) REFERENCES `associados` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 8. Tabela: espacos (Seção 5.8)
CREATE TABLE IF NOT EXISTS `espacos` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_clube` INT NOT NULL,
  `nome` VARCHAR(150) NOT NULL,
  `descricao` TEXT NULL,
  `capacidade` INT NULL,
  `ativo` TINYINT(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`),
  KEY `fk_espacos_clube` (`id_clube`),
  CONSTRAINT `fk_espacos_clube` FOREIGN KEY (`id_clube`) REFERENCES `clubes` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 9. Tabela: reservas (Seção 5.9)
CREATE TABLE IF NOT EXISTS `reservas` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_clube` INT NOT NULL,
  `id_espaco` INT NOT NULL,
  `id_associado` INT NOT NULL,
  `titulo` VARCHAR(200) NULL,
  `data_hora_inicio` DATETIME NOT NULL,
  `data_hora_fim` DATETIME NOT NULL,
  `finalidade` VARCHAR(200) NULL,
  `status` VARCHAR(20) NOT NULL DEFAULT 'PENDENTE' COMMENT 'PENDENTE, APROVADA, REJEITADA, CANCELADA',
  `observacao` TEXT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_reservas_clube` (`id_clube`),
  KEY `fk_reservas_espaco` (`id_espaco`),
  KEY `fk_reservas_associado` (`id_associado`),
  CONSTRAINT `fk_reservas_clube` FOREIGN KEY (`id_clube`) REFERENCES `clubes` (`id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_reservas_espaco` FOREIGN KEY (`id_espaco`) REFERENCES `espacos` (`id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_reservas_associado` FOREIGN KEY (`id_associado`) REFERENCES `associados` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 10. Tabela: solicitacoes (Seção 5.10)
CREATE TABLE IF NOT EXISTS `solicitacoes` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_clube` INT NOT NULL,
  `id_associado` INT NULL,
  `tipo` VARCHAR(50) NOT NULL,
  `descricao` TEXT NOT NULL,
  `data_abertura` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `status` VARCHAR(20) NOT NULL DEFAULT 'ABERTA' COMMENT 'ABERTA, EM_ANALISE, RESPONDIDA, ENCERRADA',
  `resposta` TEXT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_solicitacoes_clube` (`id_clube`),
  KEY `fk_solicitacoes_associado` (`id_associado`),
  CONSTRAINT `fk_solicitacoes_clube` FOREIGN KEY (`id_clube`) REFERENCES `clubes` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_solicitacoes_associado` FOREIGN KEY (`id_associado`) REFERENCES `associados` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 11. Tabela: reclamacoes (Seção 5.11)
CREATE TABLE IF NOT EXISTS `reclamacoes` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `id_clube` INT NOT NULL,
  `id_associado` INT NULL,
  `assunto` VARCHAR(200) NULL,
  `descricao` TEXT NOT NULL,
  `data_abertura` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `status` VARCHAR(20) NOT NULL DEFAULT 'ABERTA' COMMENT 'ABERTA, EM_ANALISE, RESPONDIDA, ENCERRADA',
  `resposta` TEXT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_reclamacoes_clube` (`id_clube`),
  KEY `fk_reclamacoes_associado` (`id_associado`),
  CONSTRAINT `fk_reclamacoes_clube` FOREIGN KEY (`id_clube`) REFERENCES `clubes` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_reclamacoes_associado` FOREIGN KEY (`id_associado`) REFERENCES `associados` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ====================================================================
-- SEED DE DADOS INICIAIS
-- ====================================================================

-- Clubes
INSERT INTO `clubes` (`id`, `nome`, `sigla`, `endereco`, `bairro`, `cidade`, `uf`, `telefone`, `email`, `ativo`) VALUES
(1, 'Clube Social Cascavel', 'CSC', 'Av. Brasil, 4500', 'Centro', 'Cascavel', 'PR', '(45) 3224-1000', 'contato@clubesocialcascavel.com.br', 1),
(2, 'Iate Clube Foz do Iguaçu', 'ICFI', 'Av. das Cataratas, 1200', 'Vila Yolanda', 'Foz do Iguaçu', 'PR', '(45) 3523-2000', 'atendimento@iateclubefoz.com.br', 1),
(3, 'Clube Náutico Guaíra', 'CNG', 'Rua Beira Rio, 350', 'Porto Velho', 'Guaíra', 'PR', '(44) 3642-3000', 'secretaria@nauticoguaira.com.br', 1);

-- Usuários (Senhas hash bcrypt: Admin@123 ou User@123)
INSERT INTO `usuarios` (`id`, `nome`, `email`, `senha_hash`, `perfil`, `id_clube`, `ativo`) VALUES
(1, 'Super Administrador Geral', 'superadmin@clubnet.com', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'SUPERADMIN', NULL, 1),
(2, 'Roberto Menezes (Presidente)', 'presidente.cascavel@clubnet.com', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'PRESIDENTE', 1, 1),
(3, 'Fernanda Souza (Administradora)', 'admin.cascavel@clubnet.com', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'ADMIN', 1, 1),
(4, 'Paulo Ricardo (Tesoureiro)', 'tesoureiro.cascavel@clubnet.com', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'TESOUREIRO', 1, 1),
(5, 'Carlos Eduardo Silva', 'carlos.silva@email.com', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'ASSOCIADO', 1, 1);
