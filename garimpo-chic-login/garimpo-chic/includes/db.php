<?php
// includes/db.php
// Configurações do banco de dados — edite conforme seu ambiente

define('DB_HOST', 'localhost');
define('DB_NAME', 'garimpo_chic');
define('DB_USER', 'root');       // troque pelo seu usuário
define('DB_PASS', '');           // troque pela sua senha
define('DB_CHARSET', 'utf8mb4');

try {
    $dsn = 'mysql:host=' . DB_HOST . ';dbname=' . DB_NAME . ';charset=' . DB_CHARSET;
    $opcoes = [
        PDO::ATTR_ERRMODE            => PDO::ERRMODE_EXCEPTION,
        PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
        PDO::ATTR_EMULATE_PREPARES   => false,
    ];
    $pdo = new PDO($dsn, DB_USER, DB_PASS, $opcoes);
} catch (PDOException $e) {
    // Em produção, não exponha detalhes do erro
    error_log('Erro de conexão: ' . $e->getMessage());
    die('Erro interno. Tente novamente mais tarde.');
}
