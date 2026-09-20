<!DOCTYPE html>
<html lang="pt-BR">

<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Criar conta — O Garimpo Chic</title>
  <link rel="stylesheet" href="./src/css/logincli.css">
  <link rel="stylesheet" href="./src/css/cadastro.css">
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

    <h1 class="titulo-cadastro">Criar minha conta</h1>

    <form action="index.php" method="POST" id="form-cadastro" novalidate>

      <div class="campo">
        <label for="nome">Nome completo</label>
        <input type="text" id="nome" name="nome" placeholder="Seu nome" autocomplete="name">
        <span class="erro-campo" id="erro-nome">Informe seu nome completo.</span>
      </div>

      <div class="campo">
        <label for="cpf">CPF</label>
        <input type="text" id="cpf" name="cpf" placeholder="000.000.000-00" maxlength="14">
        <span class="erro-campo" id="erro-cpf">Informe um CPF válido.</span>
      </div>

      <div class="campo">
        <label for="email">E-mail</label>
        <input type="email" id="email" name="email" placeholder="voce@email.com" autocomplete="email">
        <span class="erro-campo" id="erro-email">Informe um e-mail válido.</span>
      </div>

      <div class="campo">
        <label for="telefone">Telefone</label>
        <input type="text" id="telefone" name="telefone" placeholder="(00) 00000-0000" maxlength="15"
          autocomplete="tel">
        <span class="erro-campo" id="erro-telefone">Informe um telefone válido.</span>
      </div>

      <div class="campo">
        <label for="senha">Senha</label>
        <input type="password" id="senha" name="senha" placeholder="••••••••" autocomplete="new-password">
        <span class="erro-campo" id="erro-senha">Use pelo menos 8 caracteres.</span>
      </div>

      <div class="campo">
        <label for="confirma">Confirmar senha</label>
        <input type="password" id="confirma" placeholder="••••••••" autocomplete="new-password">
        <span class="erro-campo" id="erro-confirma">As senhas não coincidem.</span>
      </div>

      <button type="submit" class="btn-entrar">Criar conta</button>

    </form>

    <div class="area-link">
      <p>Já tem uma conta?</p>
      <a href="index.php">Entrar →</a>
    </div>

  </div>

  <script>
    const cpf = document.getElementById('cpf');
    const telefone = document.getElementById('telefone');

    cpf.addEventListener('input', () => {
      cpf.value = cpf.value.replace(/\D/g, '').slice(0, 11)
        .replace(/(\d{3})(\d)/, '$1.$2')
        .replace(/(\d{3})(\d)/, '$1.$2')
        .replace(/(\d{3})(\d{1,2})$/, '$1-$2');
    });

    telefone.addEventListener('input', () => {
      telefone.value = telefone.value.replace(/\D/g, '').slice(0, 11)
        .replace(/^(\d{2})(\d)/, '($1) $2')
        .replace(/(\d{5})(\d{1,4})$/, '$1-$2');
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

    function mostrar(id, ok) {
      document.getElementById(id).style.display = ok ? 'none' : 'block';
      return ok;
    }

    document.getElementById('form-cadastro').addEventListener('submit', e => {
      const nome = document.getElementById('nome').value.trim();
      const email = document.getElementById('email').value;
      const senha = document.getElementById('senha').value;
      const confirma = document.getElementById('confirma').value;

      const ok = [
        mostrar('erro-nome', nome.split(/\s+/).length >= 2),
        mostrar('erro-cpf', cpfValido(cpf.value)),
        mostrar('erro-email', /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)),
        mostrar('erro-telefone', telefone.value.replace(/\D/g, '').length >= 10),
        mostrar('erro-senha', senha.length >= 8),
        mostrar('erro-confirma', senha === confirma && confirma !== '')
      ];

      if (ok.includes(false)) e.preventDefault();
    });
  </script>
</body>

</html>