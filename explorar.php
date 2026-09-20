<!DOCTYPE html>
<html lang="pt-BR">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <title>Explorar — Garimpo Chic</title>

    <link rel="stylesheet" href="./src/css/dashboard_cliente.css">
    <link rel="stylesheet" href="./src/css/explorar.css">

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

                <a href="explorar.php" class="ativo">
                    <span class="icone">🔎</span>
                    <span class="texto-menu">Explorar</span>
                </a>

                <a href="favoritos.php">
                    <span class="icone">🖤</span>
                    <span class="texto-menu">Favoritos</span>
                </a>

                <a href="compras.php">
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
                <h2>Explorar</h2>
                <p>Encontre sua próxima peça favorita</p>
            </div>

            <div class="foto">J</div>
        </div>

        <!-- BUSCA + ORDENAR -->

        <div class="explorar-barra">

            <div class="busca">
                <span class="busca-icone">🔎</span>
                <input type="text" id="busca" placeholder="Buscar por nome da peça...">
            </div>

            <select id="ordenar" class="ordenar">
                <option value="relevancia">Relevância</option>
                <option value="menor">Menor preço</option>
                <option value="maior">Maior preço</option>
            </select>

        </div>

        <!-- CATEGORIAS -->

        <div class="chips">
            <button class="chip ativo" data-categoria="todos">Todos</button>
            <button class="chip" data-categoria="roupas">Roupas</button>
            <button class="chip" data-categoria="vestidos">Vestidos</button>
            <button class="chip" data-categoria="bolsas">Bolsas</button>
            <button class="chip" data-categoria="calcados">Calçados</button>
            <button class="chip" data-categoria="acessorios">Acessórios</button>
        </div>

        <p class="contador" id="contador"></p>

        <!-- PRODUTOS -->

        <div class="produtos" id="lista">

            <div class="produto" data-nome="Blusa Vintage" data-categoria="roupas" data-preco="79.90">
                <div class="produto-img">👚</div>
                <div class="produto-info">
                    <div>
                        <h4>Blusa Vintage</h4>
                        <p>Peça exclusiva</p>
                    </div>
                    <button class="favorito">♡</button>
                </div>
                <span class="preco">R$ 79,90</span>
            </div>

            <div class="produto" data-nome="Vestido Midi Floral" data-categoria="vestidos" data-preco="139.90">
                <div class="produto-img">👗</div>
                <div class="produto-info">
                    <div>
                        <h4>Vestido Midi Floral</h4>
                        <p>Tamanho M</p>
                    </div>
                    <button class="favorito">♡</button>
                </div>
                <span class="preco">R$ 139,90</span>
            </div>

            <div class="produto" data-nome="Bolsa Retrô" data-categoria="bolsas" data-preco="129.90">
                <div class="produto-img">👜</div>
                <div class="produto-info">
                    <div>
                        <h4>Bolsa Retrô</h4>
                        <p>Nova coleção</p>
                    </div>
                    <button class="favorito">♡</button>
                </div>
                <span class="preco">R$ 129,90</span>
            </div>

            <div class="produto" data-nome="Salto Clássico" data-categoria="calcados" data-preco="159.90">
                <div class="produto-img">👠</div>
                <div class="produto-info">
                    <div>
                        <h4>Salto Clássico</h4>
                        <p>Última unidade</p>
                    </div>
                    <button class="favorito">♡</button>
                </div>
                <span class="preco">R$ 159,90</span>
            </div>

            <div class="produto" data-nome="Casaco de Lã" data-categoria="roupas" data-preco="189.90">
                <div class="produto-img">🧥</div>
                <div class="produto-info">
                    <div>
                        <h4>Casaco de Lã</h4>
                        <p>Tamanho G</p>
                    </div>
                    <button class="favorito">♡</button>
                </div>
                <span class="preco">R$ 189,90</span>
            </div>

            <div class="produto" data-nome="Calça Jeans Reta" data-categoria="roupas" data-preco="99.90">
                <div class="produto-img">👖</div>
                <div class="produto-info">
                    <div>
                        <h4>Calça Jeans Reta</h4>
                        <p>Tamanho 38</p>
                    </div>
                    <button class="favorito">♡</button>
                </div>
                <span class="preco">R$ 99,90</span>
            </div>

            <div class="produto" data-nome="Chapéu Palha" data-categoria="acessorios" data-preco="49.90">
                <div class="produto-img">👒</div>
                <div class="produto-info">
                    <div>
                        <h4>Chapéu Palha</h4>
                        <p>Peça exclusiva</p>
                    </div>
                    <button class="favorito">♡</button>
                </div>
                <span class="preco">R$ 49,90</span>
            </div>

            <div class="produto" data-nome="Óculos Vintage" data-categoria="acessorios" data-preco="59.90">
                <div class="produto-img">🕶️</div>
                <div class="produto-info">
                    <div>
                        <h4>Óculos Vintage</h4>
                        <p>Anos 80</p>
                    </div>
                    <button class="favorito">♡</button>
                </div>
                <span class="preco">R$ 59,90</span>
            </div>

            <div class="produto" data-nome="Vestido Preto Clássico" data-categoria="vestidos" data-preco="119.90">
                <div class="produto-img">👗</div>
                <div class="produto-info">
                    <div>
                        <h4>Vestido Preto Clássico</h4>
                        <p>Tamanho P</p>
                    </div>
                    <button class="favorito">♡</button>
                </div>
                <span class="preco">R$ 119,90</span>
            </div>

        </div>

        <p class="vazio" id="vazio">Nenhuma peça encontrada ✦</p>

    </main>

    <script>
        const lista = document.getElementById('lista');
        const produtos = Array.from(document.querySelectorAll('.produto'));
        const busca = document.getElementById('busca');
        const ordenar = document.getElementById('ordenar');
        const chips = document.querySelectorAll('.chip');
        const contador = document.getElementById('contador');
        const vazio = document.getElementById('vazio');

        let categoriaAtual = 'todos';

        function atualizar() {
            const texto = busca.value.trim().toLowerCase();

            let visiveis = produtos.filter(p => {
                const nome = p.dataset.nome.toLowerCase();
                const okBusca = nome.includes(texto);
                const okCategoria = categoriaAtual === 'todos' || p.dataset.categoria === categoriaAtual;
                return okBusca && okCategoria;
            });

            if (ordenar.value === 'menor') {
                visiveis.sort((a, b) => a.dataset.preco - b.dataset.preco);
            } else if (ordenar.value === 'maior') {
                visiveis.sort((a, b) => b.dataset.preco - a.dataset.preco);
            }

            produtos.forEach(p => p.style.display = 'none');
            visiveis.forEach(p => {
                p.style.display = '';
                lista.appendChild(p);
            });

            contador.textContent = visiveis.length + (visiveis.length === 1 ? ' peça encontrada' : ' peças encontradas');
            vazio.style.display = visiveis.length === 0 ? 'block' : 'none';
        }

        chips.forEach(chip => {
            chip.addEventListener('click', () => {
                chips.forEach(c => c.classList.remove('ativo'));
                chip.classList.add('ativo');
                categoriaAtual = chip.dataset.categoria;
                atualizar();
            });
        });

        busca.addEventListener('input', atualizar);
        ordenar.addEventListener('change', atualizar);

        document.querySelectorAll('.favorito').forEach(btn => {
            btn.addEventListener('click', () => {
                btn.classList.toggle('ativo');
                btn.textContent = btn.classList.contains('ativo') ? '♥' : '♡';
            });
        });

        atualizar();
    </script>

</body>

</html>