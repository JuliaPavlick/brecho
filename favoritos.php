<!DOCTYPE html>
<html lang="pt-BR">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <title>Favoritos — Garimpo Chic</title>

    <link rel="stylesheet" href="./src/css/dashboard_cliente.css">
    <link rel="stylesheet" href="./src/css/explorar.css">
    <link rel="stylesheet" href="./src/css/favoritos.css">

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

                <a href="favoritos.php" class="ativo">
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

                <a href="perfil.php">
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
                <h2>Favoritos</h2>
                <p>As peças que conquistaram você</p>
            </div>

            <div class="foto">J</div>
        </div>

        <p class="contador" id="contador"></p>

        <!-- PRODUTOS -->

        <div class="produtos" id="lista">

            <div class="produto">
                <div class="produto-img">👚</div>
                <div class="produto-info">
                    <div>
                        <h4>Blusa Vintage</h4>
                        <p>Peça exclusiva</p>
                    </div>
                    <button class="favorito ativo" title="Remover dos favoritos">♥</button>
                </div>
                <span class="preco">R$ 79,90</span>
            </div>

            <div class="produto">
                <div class="produto-img">👗</div>
                <div class="produto-info">
                    <div>
                        <h4>Vestido Midi Floral</h4>
                        <p>Tamanho M</p>
                    </div>
                    <button class="favorito ativo" title="Remover dos favoritos">♥</button>
                </div>
                <span class="preco">R$ 139,90</span>
            </div>

            <div class="produto">
                <div class="produto-img">👜</div>
                <div class="produto-info">
                    <div>
                        <h4>Bolsa Retrô</h4>
                        <p>Nova coleção</p>
                    </div>
                    <button class="favorito ativo" title="Remover dos favoritos">♥</button>
                </div>
                <span class="preco">R$ 129,90</span>
            </div>

            <div class="produto">
                <div class="produto-img">🕶️</div>
                <div class="produto-info">
                    <div>
                        <h4>Óculos Vintage</h4>
                        <p>Anos 80</p>
                    </div>
                    <button class="favorito ativo" title="Remover dos favoritos">♥</button>
                </div>
                <span class="preco">R$ 59,90</span>
            </div>

        </div>

        <!-- ESTADO VAZIO -->

        <div class="vazio-favoritos" id="vazio">
            <div class="vazio-icone">🖤</div>
            <h3>Nenhum favorito por aqui</h3>
            <p>Toque no coração das peças que você ama para guardá-las nesta lista.</p>
            <a href="explorar.php">Explorar peças</a>
        </div>

    </main>

    <script>
        const lista = document.getElementById('lista');
        const contador = document.getElementById('contador');
        const vazio = document.getElementById('vazio');

        function atualizar() {
            const total = lista.querySelectorAll('.produto').length;

            contador.textContent = total + (total === 1 ? ' peça favorita' : ' peças favoritas');
            contador.style.display = total === 0 ? 'none' : 'block';
            lista.style.display = total === 0 ? 'none' : '';
            vazio.style.display = total === 0 ? 'block' : 'none';
        }

        lista.addEventListener('click', e => {
            const btn = e.target.closest('.favorito');
            if (!btn) return;

            const card = btn.closest('.produto');
            card.classList.add('saindo');

            setTimeout(() => {
                card.remove();
                atualizar();
            }, 250);
        });

        atualizar();
    </script>

</body>

</html>