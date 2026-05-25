<!DOCTYPE html>
<html lang="pt-BR">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <title>Dashboard Cliente — Garimpo Chic</title>

    <link rel="stylesheet" href="./src/css/dashboard_cliente.css">

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

            <!-- LOGO -->

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

            <!-- MENU -->

            <nav class="menu">

                <a href="#" class="ativo">
                    <span class="icone">🏠</span>
                    <span class="texto-menu">Início</span>
                </a>

                <a href="#">
                    <span class="icone">🔎</span>
                    <span class="texto-menu">Explorar</span>
                </a>

                <a href="#">
                    <span class="icone">🖤</span>
                    <span class="texto-menu">Favoritos</span>
                </a>

                <a href="#">
                    <span class="icone">🛍️</span>
                    <span class="texto-menu">Compras</span>
                </a>

                <a href="#">
                    <span class="icone">👤</span>
                    <span class="texto-menu">Perfil</span>
                </a>

            </nav>

        </div>

        <!-- LOGOUT -->

        <div class="logout">

            <a href="login.php">

                <span class="icone">↩</span>
                <span class="texto-menu">Logout</span>

            </a>

        </div>

    </aside>

    <!-- CONTEÚDO -->

    <main class="conteudo">

        <!-- TOPO -->

        <div class="topo">

            <div>

                <h2>Olá, Julia ✨</h2>

                <p>
                    Bem-vinda ao Garimpo Chic
                </p>

            </div>

            <div class="acoes-topo">

                <button class="botao-topo">
                    Novidades
                </button>

                <div class="foto">
                    J
                </div>

            </div>

        </div>

        <!-- HERO -->

        <section class="hero">

            <div class="hero-texto">

                <span>NOVA COLEÇÃO</span>

                <h1>
                    Peças únicas escolhidas com carinho
                </h1>

                <p>
                    Descubra novas peças vintage, elegantes e exclusivas disponíveis no brechó.
                </p>

                <button>
                    Explorar agora
                </button>

            </div>

            <div class="hero-imagem">
                👗
            </div>

        </section>

        <!-- PRODUTOS -->

        <section class="secao">

            <div class="secao-topo">

                <h3>Peças para você</h3>

                <a href="#">
                    Ver tudo
                </a>

            </div>

            <div class="produtos">

                <!-- PRODUTO -->

                <div class="produto">

                    <div class="produto-img">
                        👚
                    </div>

                    <div class="produto-info">

                        <div>

                            <h4>Blusa Vintage</h4>

                            <p>Peça exclusiva</p>

                        </div>

                        <button class="favorito">
                            ♡
                        </button>

                    </div>

                    <span class="preco">
                        R$ 79,90
                    </span>

                </div>

                <!-- PRODUTO -->

                <div class="produto">

                    <div class="produto-img">
                        👜
                    </div>

                    <div class="produto-info">

                        <div>

                            <h4>Bolsa Retrô</h4>

                            <p>Nova coleção</p>

                        </div>

                        <button class="favorito">
                            ♡
                        </button>

                    </div>

                    <span class="preco">
                        R$ 129,90
                    </span>

                </div>

                <!-- PRODUTO -->

                <div class="produto">

                    <div class="produto-img">
                        👠
                    </div>

                    <div class="produto-info">

                        <div>

                            <h4>Salto Clássico</h4>

                            <p>Última unidade</p>

                        </div>

                        <button class="favorito">
                            ♡
                        </button>

                    </div>

                    <span class="preco">
                        R$ 159,90
                    </span>

                </div>

            </div>

        </section>

    </main>

</body>

</html>