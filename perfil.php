<!DOCTYPE html>
<html lang="pt-BR">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <title>Perfil — Garimpo Chic</title>

    <link rel="stylesheet" href="./src/css/dashboard_cliente.css">
    <link rel="stylesheet" href="./src/css/perfil.css">

    <link
        href="https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,400;0,600;1,400&family=Lato:wght@300;400;700&display=swap"
        rel="stylesheet">
</head>

<body>

    <!-- LINHA SUPERIOR -->

    <div class="linha-topo">
        <div class="decoracao">
            <div class="traco"></div>
            <span class="brilho">✦</span>
            <span class="brilho">✦</span>
            <div class="traco"></div>
        </div>
    </div>

    <!-- LINHA INFERIOR -->

    <div class="linha-baixo">
        <div class="decoracao">
            <div class="traco"></div>
            <span class="brilho">✦</span>
            <span class="brilho">✦</span>
            <div class="traco"></div>
        </div>
    </div>

    <!-- SIDEBAR -->

    <aside class="sidebar">

        <div>

            <div class="logo">

                <div class="logo-circulo">
                    <div class="logo-interno">
                        <span class="logo-o">O</span>
                        <span class="logo-garimpo">Garimpo</span>
                        <span class="logo-chic">Chic</span>
                        <span class="logo-brecho">Brechó</span>
                    </div>
                </div>

                <div class="logo-texto">
                    <h1>Garimpo Chic</h1>
                    <p>Curado com carinho</p>
                </div>

            </div>

            <nav class="menu">

                <a href="dashboard_cliente.php">
                    <span class="icone">
                        <img src="./src/img/casa.png" alt="Dashboard">
                    </span>
                    <span class="texto-menu">Início</span>
                </a>

                <a href="explorar.php">
                    <span class="icone">
                        <img src="./src/img/lupa.png" alt="Dashboard">
                    </span>
                    <span class="texto-menu">Explorar</span>
                </a>

                <a href="favoritos.php">
                    <span class="icone">
                        <img src="./src/img/coração.png" alt="Dashboard">
                    </span>
                    <span class="texto-menu">Favoritos</span>
                </a>

                <a href="compras.php">
                    <span class="icone">
                        <img src="./src/img/bolsa.png" alt="Dashboard">
                    </span>
                    <span class="texto-menu">Compras</span>
                </a>

                <a href="perfil.php" class="ativo">
                    <span class="icone">
                        <img src="./src/img/pessoa.png" alt="Dashboard">
                    </span>
                    <span class="texto-menu">Perfil</span>
                </a>

            </nav>

        </div>

        <div class="logout">
            <a href="index.php">
                <span class="icone">↩</span>
                <span class="texto-menu">Logout</span>
            </a>
        </div>

    </aside>

    <!-- CONTEÚDO -->

    <main class="conteudo">

        <div class="topo">
            <div>
                <h2>Meu perfil</h2>
                <p>Mantenha seus dados sempre atualizados</p>
            </div>

            <div class="foto">J</div>
        </div>

        <div class="perfil-grid">

            <!-- RESUMO -->

            <aside class="perfil-resumo">

                <div class="avatar">J</div>

                <h3>Julia</h3>
                <p class="resumo-sub">Cliente Garimpo Chic</p>

                <div class="resumo-numeros">
                    <div>
                        <strong>4</strong>
                        <span>Pedidos</span>
                    </div>
                    <div>
                        <strong>4</strong>
                        <span>Favoritos</span>
                    </div>
                </div>

            </aside>

            <!-- FORMULÁRIOS -->

            <div class="perfil-forms">

                <form class="painel-form" id="form-dados" novalidate>

                    <h3>Dados pessoais</h3>

                    <div class="linha-campos">
                        <div class="campo">
                            <label for="nome">Nome</label>
                            <input type="text" id="nome" value="Julia">
                            <span class="erro-campo" id="erro-nome">Informe seu nome.</span>
                        </div>

                        <div class="campo">
                            <label for="cpf">CPF</label>
                            <input type="text" id="cpf" value="529.982.247-25" disabled>
                        </div>
                    </div>

                    <div class="linha-campos">
                        <div class="campo">
                            <label for="email">E-mail</label>
                            <input type="email" id="email" value="julia@email.com">
                            <span class="erro-campo" id="erro-email">Informe um e-mail válido.</span>
                        </div>

                        <div class="campo">
                            <label for="telefone">Telefone</label>
                            <input type="text" id="telefone" value="(47) 99999-9999" maxlength="15">
                            <span class="erro-campo" id="erro-telefone">Informe um telefone válido.</span>
                        </div>
                    </div>

                    <div class="linha-campos">
                        <div class="campo">
                            <label for="cidade">Cidade</label>
                            <input type="text" id="cidade" value="Joinville">
                        </div>

                        <div class="campo">
                            <label for="estado">Estado</label>
                            <input type="text" id="estado" value="SC" maxlength="2">
                        </div>
                    </div>

                    <div class="acoes-form">
                        <span class="msg-sucesso" id="msg-dados">Dados salvos ✦</span>
                        <button type="submit" class="btn-salvar">Salvar alterações</button>
                    </div>

                </form>

                <form class="painel-form" id="form-senha" novalidate>

                    <h3>Alterar senha</h3>

                    <div class="campo">
                        <label for="senha-atual">Senha atual</label>
                        <input type="password" id="senha-atual" autocomplete="current-password">
                        <span class="erro-campo" id="erro-senha-atual">Informe sua senha atual.</span>
                    </div>

                    <div class="linha-campos">
                        <div class="campo">
                            <label for="senha-nova">Nova senha</label>
                            <input type="password" id="senha-nova" autocomplete="new-password">
                            <span class="erro-campo" id="erro-senha-nova">Use pelo menos 8 caracteres.</span>
                        </div>

                        <div class="campo">
                            <label for="senha-confirma">Confirmar nova senha</label>
                            <input type="password" id="senha-confirma" autocomplete="new-password">
                            <span class="erro-campo" id="erro-senha-confirma">As senhas não coincidem.</span>
                        </div>
                    </div>

                    <div class="acoes-form">
                        <span class="msg-sucesso" id="msg-senha">Senha alterada ✦</span>
                        <button type="submit" class="btn-salvar">Alterar senha</button>
                    </div>

                </form>

            </div>

        </div>

    </main>

    <script>
        function mostrar(id, ok) {
            document.getElementById(id).style.display = ok ? 'none' : 'block';
            return ok;
        }

        function confirmar(id) {
            const msg = document.getElementById(id);
            msg.style.display = 'inline';
            setTimeout(() => msg.style.display = 'none', 3000);
        }

        const telefone = document.getElementById('telefone');

        telefone.addEventListener('input', () => {
            telefone.value = telefone.value.replace(/\D/g, '').slice(0, 11)
                .replace(/^(\d{2})(\d)/, '($1) $2')
                .replace(/(\d{5})(\d{1,4})$/, '$1-$2');
        });

        document.getElementById('form-dados').addEventListener('submit', e => {
            e.preventDefault();

            const okNome = mostrar('erro-nome', document.getElementById('nome').value.trim() !== '');
            const okEmail = mostrar('erro-email', /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(document.getElementById('email').value));
            const okTel = mostrar('erro-telefone', telefone.value.replace(/\D/g, '').length >= 10);

            if (okNome && okEmail && okTel) confirmar('msg-dados');
        });

        document.getElementById('form-senha').addEventListener('submit', e => {
            e.preventDefault();

            const atual = document.getElementById('senha-atual').value;
            const nova = document.getElementById('senha-nova').value;
            const confirma = document.getElementById('senha-confirma').value;

            const okAtual = mostrar('erro-senha-atual', atual !== '');
            const okNova = mostrar('erro-senha-nova', nova.length >= 8);
            const okConfirma = mostrar('erro-senha-confirma', nova === confirma && confirma !== '');

            if (okAtual && okNova && okConfirma) {
                e.target.reset();
                confirmar('msg-senha');
            }
        });
    </script>

</body>

</html>