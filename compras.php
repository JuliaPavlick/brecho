<!DOCTYPE html>
<html lang="pt-BR">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <title>Compras — Garimpo Chic</title>

    <link rel="stylesheet" href="./src/css/dashboard_cliente.css">
    <link rel="stylesheet" href="./src/css/explorar.css">
    <link rel="stylesheet" href="./src/css/compras.css">

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
                    <span class="icone">🏠</span>
                    <span class="texto-menu">Início</span>
                </a>

                <a href="explorar.php">
                    <span class="icone">🔎</span>
                    <span class="texto-menu">Explorar</span>
                </a>

                <a href="favoritos.php">
                    <span class="icone">🖤</span>
                    <span class="texto-menu">Favoritos</span>
                </a>

                <a href="compras.php" class="ativo">
                    <span class="icone">🛍️</span>
                    <span class="texto-menu">Compras</span>
                </a>

                <a href="perfil.php">
                    <span class="icone">👤</span>
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
                <h2>Minhas compras</h2>
                <p>Acompanhe seus pedidos no Garimpo Chic</p>
            </div>

            <div class="foto">J</div>
        </div>

        <!-- FILTROS DE STATUS -->

        <div class="chips">
            <button class="chip ativo" data-status="todos">Todos</button>
            <button class="chip" data-status="preparo">Em preparo</button>
            <button class="chip" data-status="caminho">A caminho</button>
            <button class="chip" data-status="entregue">Entregue</button>
        </div>

        <p class="contador" id="contador"></p>

        <!-- PEDIDOS -->

        <div class="pedidos" id="lista">

            <div class="pedido" data-status="preparo">
                <div class="pedido-topo">
                    <div>
                        <h4>Pedido #0004</h4>
                        <p>18 de setembro de 2026</p>
                    </div>
                    <span class="status preparo">Em preparo</span>
                </div>

                <div class="pedido-itens">
                    <div class="item">
                        <div class="item-img">🧥</div>
                        <div class="item-info">
                            <h5>Casaco de Lã</h5>
                            <p>Tamanho G</p>
                        </div>
                        <span class="item-preco">R$ 189,90</span>
                    </div>
                </div>

                <div class="pedido-rodape">
                    <span>Total</span>
                    <strong>R$ 189,90</strong>
                </div>
            </div>

            <div class="pedido" data-status="caminho">
                <div class="pedido-topo">
                    <div>
                        <h4>Pedido #0003</h4>
                        <p>12 de setembro de 2026</p>
                    </div>
                    <span class="status caminho">A caminho</span>
                </div>

                <div class="pedido-itens">
                    <div class="item">
                        <div class="item-img">🕶️</div>
                        <div class="item-info">
                            <h5>Óculos Vintage</h5>
                            <p>Anos 80</p>
                        </div>
                        <span class="item-preco">R$ 59,90</span>
                    </div>

                    <div class="item">
                        <div class="item-img">👒</div>
                        <div class="item-info">
                            <h5>Chapéu Palha</h5>
                            <p>Peça exclusiva</p>
                        </div>
                        <span class="item-preco">R$ 49,90</span>
                    </div>
                </div>

                <div class="pedido-rodape">
                    <span>Total</span>
                    <strong>R$ 109,80</strong>
                </div>
            </div>

            <div class="pedido" data-status="entregue">
                <div class="pedido-topo">
                    <div>
                        <h4>Pedido #0002</h4>
                        <p>30 de agosto de 2026</p>
                    </div>
                    <span class="status entregue">Entregue</span>
                </div>

                <div class="pedido-itens">
                    <div class="item">
                        <div class="item-img">👗</div>
                        <div class="item-info">
                            <h5>Vestido Midi Floral</h5>
                            <p>Tamanho M</p>
                        </div>
                        <span class="item-preco">R$ 139,90</span>
                    </div>
                </div>

                <div class="pedido-rodape">
                    <span>Total</span>
                    <strong>R$ 139,90</strong>
                </div>
            </div>

            <div class="pedido" data-status="entregue">
                <div class="pedido-topo">
                    <div>
                        <h4>Pedido #0001</h4>
                        <p>14 de agosto de 2026</p>
                    </div>
                    <span class="status entregue">Entregue</span>
                </div>

                <div class="pedido-itens">
                    <div class="item">
                        <div class="item-img">👚</div>
                        <div class="item-info">
                            <h5>Blusa Vintage</h5>
                            <p>Peça exclusiva</p>
                        </div>
                        <span class="item-preco">R$ 79,90</span>
                    </div>

                    <div class="item">
                        <div class="item-img">👜</div>
                        <div class="item-info">
                            <h5>Bolsa Retrô</h5>
                            <p>Nova coleção</p>
                        </div>
                        <span class="item-preco">R$ 129,90</span>
                    </div>
                </div>

                <div class="pedido-rodape">
                    <span>Total</span>
                    <strong>R$ 209,80</strong>
                </div>
            </div>

        </div>

        <p class="vazio" id="vazio">Nenhum pedido nesta categoria ✦</p>

    </main>

    <script>
        const pedidos = document.querySelectorAll('.pedido');
        const chips = document.querySelectorAll('.chip');
        const contador = document.getElementById('contador');
        const vazio = document.getElementById('vazio');

        function filtrar(status) {
            let total = 0;

            pedidos.forEach(p => {
                const mostrar = status === 'todos' || p.dataset.status === status;
                p.style.display = mostrar ? '' : 'none';
                if (mostrar) total++;
            });

            contador.textContent = total + (total === 1 ? ' pedido' : ' pedidos');
            vazio.style.display = total === 0 ? 'block' : 'none';
        }

        chips.forEach(chip => {
            chip.addEventListener('click', () => {
                chips.forEach(c => c.classList.remove('ativo'));
                chip.classList.add('ativo');
                filtrar(chip.dataset.status);
            });
        });

        filtrar('todos');
    </script>

</body>

</html>