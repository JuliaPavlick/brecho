<!DOCTYPE html>
<html lang="pt-BR">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <title>Dashboard Funcionária</title>
    <link rel="stylesheet" href="dashfunc.css">
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

            <<a href="#">

                <span class="icone">
                    <img src="grafico-preditivo.png" alt="Dashboard">
                </span>

                <span class="texto">Dashboard</span>

                </a>

                <a href="#">
                    <span class="icone">
                        <img src="vestido-de-casamento.png" alt="Dashboard">
                    </span>
                    <span class="texto">Produtos</span>
                </a>

                <a href="#">
                    <span class="icone">📦</span>
                    <span class="texto">Pedidos</span>
                </a>

                <a href="calendario.php">
                    <span class="icone">📅</span>
                    <span class="texto">Calendário</span>
                </a>

                <a href="#">
                    <span class="icone">💰</span>
                    <span class="texto">Financeiro</span>
                </a>

                <a href="#">
                    <span class="icone">👥</span>
                    <span class="texto">Clientes</span>
                </a>

        </nav>

    </aside>

    <!-- CONTEUDO -->

    <main class="conteudo">

        <!-- TOPO -->

        <div class="topo-dashboard">

            <div class="boas-vindas">

                <h2>Olá, Julia ✦</h2>

                <p>
                    Você está acessando a área de funcionária da loja.
                </p>

            </div>

            <a href="login.php" class="logout">
                Logout
            </a>

        </div>

        <!-- CARDS -->

        <section class="cards">

            <div class="card">

                <div class="card-topo">
                    <span class="card-titulo">Peças vendidas</span>
                    <span class="card-icone">🛍️</span>
                </div>

                <h3 class="card-valor">142</h3>

                <p class="card-info">
                    +18% comparado ao último mês
                </p>

            </div>

            <div class="card">

                <div class="card-topo">
                    <span class="card-titulo">Lucro bruto</span>
                    <span class="card-icone">💰</span>
                </div>

                <h3 class="card-valor">R$ 12k</h3>

                <p class="card-info">
                    Resultado mensal da loja
                </p>

            </div>

            <div class="card">

                <div class="card-topo">
                    <span class="card-titulo">Produtos ativos</span>
                    <span class="card-icone">👗</span>
                </div>

                <h3 class="card-valor">87</h3>

                <p class="card-info">
                    Itens disponíveis atualmente
                </p>

            </div>

            <div class="card">

                <div class="card-topo">
                    <span class="card-titulo">Novas vendedoras</span>
                    <span class="card-icone"><svg xmlns="http://www.w3.org/2000/svg" enable-background="new 0 0 24 24"
                            height="40px" viewBox="0 0 24 24" width="40px" fill="#1f1f1f">
                            <g>
                                <rect fill="none" height="24" width="24" x="0" />
                            </g>
                            <g>
                                <g>
                                    <polygon points="19,9 20.25,6.25 23,5 20.25,3.75 19,1 17.75,3.75 15,5 17.75,6.25" />
                                    <polygon
                                        points="19,15 17.75,17.75 15,19 17.75,20.25 19,23 20.25,20.25 23,19 20.25,17.75" />
                                    <path
                                        d="M11.5,9.5L9,4L6.5,9.5L1,12l5.5,2.5L9,20l2.5-5.5L17,12L11.5,9.5z M9.99,12.99L9,15.17l-0.99-2.18L5.83,12l2.18-0.99 L9,8.83l0.99,2.18L12.17,12L9.99,12.99z" />
                                </g>
                            </g>
                        </svg></span>
                </div>

                <h3 class="card-valor">24</h3>

                <p class="card-info">
                    Cadastros aprovados este mês
                </p>

            </div>

        </section>

        <!-- AREA INFERIOR -->

        <section class="inferior">

            <!-- GRAFICO -->

            <div class="painel">

                <h3>Movimento da Loja</h3>

                <div class="filtros">
                    <div class="filtro">Semana</div>
                    <div class="filtro">Mês</div>
                    <div class="filtro">Ano</div>
                </div>

                <div class="grafico">

                    <div class="barra" style="height:45%;">
                        <div class="tooltip">R$ 1.240</div>
                        <span>Seg</span>
                    </div>

                    <div class="barra" style="height:65%;">
                        <div class="tooltip">R$ 2.180</div>
                        <span>Ter</span>
                    </div>

                    <div class="barra" style="height:85%;">
                        <div class="tooltip">R$ 3.020</div>
                        <span>Qua</span>
                    </div>

                    <div class="barra" style="height:55%;">
                        <div class="tooltip">R$ 1.870</div>
                        <span>Qui</span>
                    </div>

                    <div class="barra" style="height:92%;">
                        <div class="tooltip">R$ 3.880</div>
                        <span>Sex</span>
                    </div>

                    <div class="barra" style="height:70%;">
                        <div class="tooltip">R$ 2.760</div>
                        <span>Sáb</span>
                    </div>

                    <div class="barra" style="height:38%;">
                        <div class="tooltip">R$ 980</div>
                        <span>Dom</span>
                    </div>

                </div>

            </div>

            <!-- EVENTOS -->

            <a href="calendario.php" class="painel-link">

                <div class="painel">

                    <h3>Próximos Eventos</h3>

                    <div class="calendario-item">
                        <strong>Hoje • 14:00</strong>
                        <p>Recebimento de peças da Ana Clara</p>
                    </div>

                    <div class="calendario-item">
                        <strong>Amanhã • 10:00</strong>
                        <p>Avaliação de roupas vintage</p>
                    </div>

                    <div class="calendario-item">
                        <strong>Sexta-feira • 10:00</strong>
                        <p>Nova coleção em destaque</p>
                    </div>

                    <div class="calendario-item">
                        <strong>Próximo feriado</strong>
                        <p>Funcionamento reduzido da loja</p>
                    </div>

                </div>

            </a>

        </section>

    </main>

</body>

</html>