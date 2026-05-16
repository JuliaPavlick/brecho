<?php
// php/auth.php — Funções de autenticação

/**
 * Verifica CPF + senha no banco.
 * Retorna array com dados do usuário ou false.
 */
function autenticar(PDO $pdo, string $cpf, string $senha): array|false
{
    $stmt = $pdo->prepare('
        SELECT id, nome, cpf, senha_hash, ativo
        FROM usuarios
        WHERE cpf = ?
        LIMIT 1
    ');
    $stmt->execute([$cpf]);
    $usuario = $stmt->fetch();

    if (!$usuario) {
        return false; // CPF não encontrado
    }

    if (!$usuario['ativo']) {
        return false; // Conta desativada
    }

    if (!password_verify($senha, $usuario['senha_hash'])) {
        return false; // Senha incorreta
    }

    return $usuario;
}

/**
 * Retorna lista de papéis do usuário.
 * Ex: ['CLIENTE', 'VENDEDORA_PARA_LOJA']
 */
function buscar_papeis(PDO $pdo, int $usuario_id): array
{
    $stmt = $pdo->prepare('
        SELECT p.nome
        FROM papeis p
        INNER JOIN usuario_papeis up ON up.papel_id = p.id
        WHERE up.usuario_id = ?
          AND up.ativo = 1
    ');
    $stmt->execute([$usuario_id]);
    return $stmt->fetchAll(PDO::FETCH_COLUMN);
}

/**
 * Registra um novo usuário.
 * Retorna o ID inserido ou false em caso de erro.
 */
function registrar_usuario(PDO $pdo, array $dados): int|false
{
    try {
        $stmt = $pdo->prepare('
            INSERT INTO usuarios (cpf, nome, email, whatsapp, senha_hash, ativo, criado_em)
            VALUES (?, ?, ?, ?, ?, 1, NOW())
        ');
        $stmt->execute([
            $dados['cpf'],
            $dados['nome'],
            $dados['email'],
            $dados['whatsapp'] ?? null,
            password_hash($dados['senha'], PASSWORD_DEFAULT),
        ]);
        return (int) $pdo->lastInsertId();
    } catch (PDOException $e) {
        error_log('Erro ao registrar usuário: ' . $e->getMessage());
        return false;
    }
}

/**
 * Adiciona um papel a um usuário.
 * Ex: adicionar_papel($pdo, 5, 'CLIENTE')
 */
function adicionar_papel(PDO $pdo, int $usuario_id, string $papel_nome): bool
{
    $stmt = $pdo->prepare('SELECT id FROM papeis WHERE nome = ?');
    $stmt->execute([$papel_nome]);
    $papel = $stmt->fetch();

    if (!$papel) return false;

    $stmt = $pdo->prepare('
        INSERT IGNORE INTO usuario_papeis (usuario_id, papel_id, ativo, criado_em)
        VALUES (?, ?, 1, NOW())
    ');
    $stmt->execute([$usuario_id, $papel['id']]);
    return true;
}

/**
 * Verifica se usuário tem determinado papel.
 */
function tem_papel(string $papel): bool
{
    return isset($_SESSION['papeis']) && in_array($papel, $_SESSION['papeis']);
}

/**
 * Redireciona se não tiver o papel exigido.
 * Use no topo de cada página protegida.
 */
function exigir_papel(string $papel): void
{
    session_start();
    if (!isset($_SESSION['usuario_id'])) {
        header('Location: /login.php');
        exit;
    }
    if (!tem_papel($papel)) {
        header('Location: /acesso-negado.php');
        exit;
    }
}
