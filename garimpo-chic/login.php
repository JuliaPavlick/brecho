<?php
session_start();

// Se já está logado, redireciona
if (isset($_SESSION['usuario_id'])) {
    header('Location: dashboard.php');
    exit;
}

$erro = '';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    require_once 'includes/db.php';
    require_once 'php/auth.php';

    $cpf   = preg_replace('/\D/', '', $_POST['cpf'] ?? '');
    $senha = $_POST['senha'] ?? '';

    if (strlen($cpf) !== 11 || empty($senha)) {
        $erro = 'Preencha o CPF e a senha.';
    } else {
        $usuario = autenticar($pdo, $cpf, $senha);
        if ($usuario) {
            $_SESSION['usuario_id']  = $usuario['id'];
            $_SESSION['usuario_nome'] = $usuario['nome'];
            $_SESSION['papeis']      = buscar_papeis($pdo, $usuario['id']);

            // Redireciona conforme papel principal
            $papeis = $_SESSION['papeis'];
            if (in_array('CLIENTE', $papeis)) {
                header('Location: cliente/dashboard.php');
            } elseif (in_array('VENDEDORA_PARA_LOJA', $papeis)) {
                header('Location: vendedor/dashboard.php');
            } elseif (in_array('FUNCIONARIA', $papeis)) {
                header('Location: admin/dashboard.php');
            } else {
                header('Location: dashboard.php');
            }
            exit;
        } else {
            $erro = 'CPF ou senha incorretos.';
        }
    }
}
?>
<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Login — O Garimpo Chic</title>
  <link rel="stylesheet" href="css/login.css">
  <link href="https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,400;0,600;1,400&family=Lato:wght@300;400;700&display=swap" rel="stylesheet">
</head>
<body>

<main class="login-page">
  <div class="login-card">

    <div class="corner-ornament tl">✦</div>
    <div class="corner-ornament tr">✦</div>
    <div class="corner-ornament bl">✦</div>
    <div class="corner-ornament br">✦</div>

    <!-- Logo -->
    <div class="logo-area">
      <div class="logo-circle">
        <div class="logo-inner">
          <span class="logo-o">O</span>
          <span class="logo-garimpo">Garimpo</span>
          <span class="logo-chic">Chic</span>
          <span class="logo-brecho">Brechó</span>
        </div>
      </div>
      <p class="logo-tagline">Curado com carinho</p>
    </div>

    <div class="divider">
      <div class="divider-line"></div>
      <span class="divider-icon">✿</span>
      <div class="divider-line"></div>
    </div>

    <!-- Erro -->
    <?php if ($erro): ?>
      <div class="alert-erro"><?= htmlspecialchars($erro) ?></div>
    <?php endif; ?>

    <!-- Formulário -->
    <form method="POST" action="login.php" novalidate>
      <div class="form-group">
        <label for="cpf">CPF</label>
        <input type="text" id="cpf" name="cpf" placeholder="000.000.000-00" maxlength="14" autocomplete="username" required>
        <span class="field-erro" id="cpf-erro"></span>
      </div>

      <div class="form-group">
        <label for="senha">Senha</label>
        <input type="password" id="senha" name="senha" placeholder="••••••••" autocomplete="current-password" required>
        <span class="field-erro" id="senha-erro"></span>
      </div>

      <button type="submit" class="btn-login">Entrar</button>
    </form>

    <!-- Links -->
    <div class="links-area">
      <span class="link-texto">Ainda não tem conta?</span>
      <a href="cadastro.php" class="link-btn">Criar minha conta →</a>
    </div>

    <div class="footer-links">
      <a href="cadastro.php?tipo=vendedor" class="footer-link">
        <span class="footer-icon">👗</span>
        Quero vender
      </a>
      <a href="cadastro.php?tipo=cliente" class="footer-link">
        <span class="footer-icon">🛍️</span>
        Quero comprar
      </a>
      <a href="recuperar-senha.php" class="footer-link">
        <span class="footer-icon">🔑</span>
        Esqueci a senha
      </a>
    </div>

  </div>
</main>

<script src="js/login.js"></script>
</body>
</html>
