<!DOCTYPE html>
<html lang="pt-BR">

<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Login — O Garimpo Chic</title>
  <link rel="stylesheet" href="./src/css/logincli.css">
  <link
    href="https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,400;0,600;1,400&family=Lato:wght@300;400;700&display=swap"
    rel="stylesheet">
</head>

<body>
  <div class="card">

    <span class="ornamento topo-esq">✦</span>
    <span class="ornamento topo-dir">✦</span>
    <span class="ornamento baixo-esq">✦</span>
    <span class="ornamento baixo-dir">✦</span>

    <div class="logo-area">
      <div class="logo-circulo">
        <div class="logo-interno">
          <span class="logo-o">O</span>
          <span class="logo-garimpo">Garimpo</span>
          <span class="logo-chic">Chic</span>
          <span class="logo-brecho">Brechó</span>
        </div>
      </div>
      <p class="logo-slogan">Curado com carinho</p>
    </div>

    <div class="divisor">
      <div class="divisor-linha"></div>
      <span class="divisor-flor">✿</span>
      <div class="divisor-linha"></div>
    </div>

    <form action="selecao_perfil.php" method="POST">

      <div class="campo">
        <label for="cpf">CPF</label>
        <input type="text" id="cpf" name="cpf" placeholder="000.000.000-00" maxlength="14" autocomplete="username">
        <span class="erro-campo" id="erro-cpf">Informe um CPF válido.</span>
      </div>

      <div class="campo">
        <label for="senha">Senha</label>
        <input type="password" id="senha" name="senha" placeholder="••••••••" autocomplete="current-password">
        <span class="erro-campo" id="erro-senha">Preencha sua senha.</span>
      </div>

      <button type="submit" class="btn-entrar">Entrar</button>

    </form>

    <div class="area-link">
      <p>Ainda não tem conta?</p>
      <a href="cadastro.php">Criar minha conta →</a>
    </div>

    <div class="rodape-login">
      <a href="recuperar-senha.php">
        Esqueci minha senha
      </a>
    </div>

  </div>
  <script>
  const form = document.querySelector('form');
  const cpf = document.getElementById('cpf');
  const senha = document.getElementById('senha');

  cpf.addEventListener('input', () => {
    cpf.value = cpf.value.replace(/\D/g, '').slice(0, 11)
      .replace(/(\d{3})(\d)/, '$1.$2')
      .replace(/(\d{3})(\d)/, '$1.$2')
      .replace(/(\d{3})(\d{1,2})$/, '$1-$2');
  });

  function cpfValido(valor) {
    const n = valor.replace(/\D/g, '');
    if (n.length !== 11 || /^(\d)\1+$/.test(n)) return false;
    for (let t = 9; t < 11; t++) {
      let soma = 0;
      for (let i = 0; i < t; i++) soma += n[i] * (t + 1 - i);
      if (((soma * 10) % 11) % 10 != n[t]) return false;
    }
    return true;
  }

  form.addEventListener('submit', e => {
    const okCpf = cpfValido(cpf.value);
    const okSenha = senha.value.trim() !== '';
    document.getElementById('erro-cpf').style.display = okCpf ? 'none' : 'block';
    document.getElementById('erro-senha').style.display = okSenha ? 'none' : 'block';
    if (!okCpf || !okSenha) e.preventDefault();
  });
</script>
</body>

</html>