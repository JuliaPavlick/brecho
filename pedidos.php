<!DOCTYPE html>
<html lang="pt-BR">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <title>Pedidos — Garimpo Chic</title>
    <link rel="stylesheet" href="./src/css/dashfunc.css">
    <link rel="stylesheet" href="./src/css/produtos.css">
    <link rel="stylesheet" href="./src/css/pedidos.css">
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

            <a href="dashboard_funcionario.php">
                <span class="icone">
                    <img src="./src/img/grafico-preditivo.png" alt="Dashboard">
                </span>
                <span class="texto">Dashboard</span>
            </a>

            <a href="produtos.php">
                <span class="icone">
                    <img src="./src/img/vestido-de-casamento.png" alt="Produtos">
                </span>
                <span class="texto">Produtos</span>
            </a>

            <a href="pedidos.php" class="ativo">
                <span class="icone">
                    <img src="./src/img/caixa.png" alt="Pedidos">
                </span>
                <span class="texto">Pedidos</span>
            </a>

            <a href="calendario.php">
                <span class="icone">
                    <img src="./src/img/calendario.png" alt="Calendário">
                </span>
                <span class="texto">Calendário</span>
            </a>

            <a href="financeiro.php">
                <span class="icone">
                    <img src="./src/img/grafico-de-crescimento.png" alt="Financeiro">
                </span>
                <span class="texto">Financeiro</span>
            </a>

            <a href="clientes.php">
                <span class="icone">
                    <img src="./src/img/pessoas.png" alt="Clientes">
                </span>
                <span class="texto">Clientes</span>
            </a>

        </nav>

    </aside>

    <!-- CONTEUDO -->

    <main class="conteudo">

        <!-- TOPO -->

        <div class="topo-dashboard">

            <div class="boas-vindas">
                <h2>Pedidos ✦</h2>
                <p>Acompanhe e atualize os pedidos da loja</p>
            </div>

            <a href="index.php" class="logout">Logout</a>

        </div>

        <!-- BUSCA -->

        <div class="barra-prod">
            <div class="busca">
                <input type="text" id="busca" placeholder="Buscar por cliente ou número do pedido...">
            </div>
        </div>

        <!-- FILTROS DE STATUS -->

        <div class="chips">
            <button class="chip ativo" data-status="todos">Todos</button>
            <button class="chip" data-status="preparo">Em preparo</button>
            <button class="chip" data-status="caminho">A caminho</button>
            <button class="chip" data-status="entregue">Entregue</button>
        </div>

        <p class="contador" id="contador"></p>

        <!-- TABELA -->

        <div class="painel">

            <div class="tabela-wrap">
                <table class="tabela">
                    <thead>
                        <tr>
                            <th>Pedido</th>
                            <th>Cliente</th>
                            <th>Data</th>
                            <th>Itens</th>
                            <th>Total</th>
                            <th>Status</th>
                            <th class="col-acoes">Ações</th>
                        </tr>
                    </thead>
                    <tbody id="corpo"></tbody>
                </table>
            </div>

            <p class="vazio" id="vazio">Nenhum pedido encontrado ✦</p>

        </div>

    </main>

    <!-- MODAL DE DETALHES -->

    <div class="modal-fundo" id="modal">

        <div class="modal">

            <h3 id="det-titulo"></h3>
            <p class="det-sub" id="det-sub"></p>

            <div id="det-itens"></div>

            <div class="det-total">
                <span>Total</span>
                <strong id="det-total"></strong>
            </div>

            <div class="acoes-modal">
                <button type="button" class="btn-secundario" id="btn-fechar">Fechar</button>
            </div>

        </div>

    </div>

    <script>
        const STATUS = {
            preparo: 'Em preparo',
            caminho: 'A caminho',
            entregue: 'Entregue'
        };

        let pedidos = [
            {
                id: 4, cliente: 'Julia', data: '18/09/2026', status: 'preparo',
                itens: [{ nome: 'Casaco de Lã', preco: 189.90 }]
            },
            {
                id: 3, cliente: 'Ana Clara', data: '12/09/2026', status: 'caminho',
                itens: [{ nome: 'Óculos Vintage', preco: 59.90 }, { nome: 'Chapéu Palha', preco: 49.90 }]
            },
            {
                id: 2, cliente: 'Mariana Souza', data: '30/08/2026', status: 'entregue',
                itens: [{ nome: 'Vestido Midi Floral', preco: 139.90 }]
            },
            {
                id: 1, cliente: 'Beatriz Lima', data: '14/08/2026', status: 'entregue',
                itens: [{ nome: 'Blusa Vintage', preco: 79.90 }, { nome: 'Bolsa Retrô', preco: 129.90 }]
            }
        ];

        let filtroStatus = 'todos';

        const corpo = document.getElementById('corpo');
        const busca = document.getElementById('busca');
        const contador = document.getElementById('contador');
        const vazio = document.getElementById('vazio');
        const modal = document.getElementById('modal');

        function esc(texto) {
            return texto.replace(/[&<>"']/g, c => ({
                '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
            }[c]));
        }

        function moeda(valor) {
            return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
        }

        function numero(id) {
            return '#' + String(id).padStart(4, '0');
        }

        function total(pedido) {
            return pedido.itens.reduce((soma, i) => soma + i.preco, 0);
        }

        function render() {
            const texto = busca.value.trim().toLowerCase();

            const lista = pedidos.filter(p =>
                (filtroStatus === 'todos' || p.status === filtroStatus) &&
                (p.cliente.toLowerCase().includes(texto) || numero(p.id).includes(texto))
            );

            corpo.innerHTML = lista.map(p => `
                <tr>
                    <td><strong>${numero(p.id)}</strong></td>
                    <td>${esc(p.cliente)}</td>
                    <td>${p.data}</td>
                    <td>${p.itens.length} ${p.itens.length === 1 ? 'peça' : 'peças'}</td>
                    <td class="preco-cel">${moeda(total(p))}</td>
                    <td>
                        <select class="status-select ${p.status}" data-id="${p.id}">
                            ${Object.keys(STATUS).map(s =>
                                `<option value="${s}" ${s === p.status ? 'selected' : ''}>${STATUS[s]}</option>`
                            ).join('')}
                        </select>
                    </td>
                    <td class="col-acoes">
                        <button class="btn-acao" data-id="${p.id}">Ver detalhes</button>
                    </td>
                </tr>
            `).join('');

            contador.textContent = lista.length + (lista.length === 1 ? ' pedido' : ' pedidos');
            vazio.style.display = lista.length === 0 ? 'block' : 'none';
        }

        function abrirDetalhes(pedido) {
            document.getElementById('det-titulo').textContent = 'Pedido ' + numero(pedido.id);
            document.getElementById('det-sub').textContent = pedido.cliente + ' • ' + pedido.data;

            document.getElementById('det-itens').innerHTML = pedido.itens.map(i => `
                <div class="det-item">
                    <span>${esc(i.nome)}</span>
                    <span>${moeda(i.preco)}</span>
                </div>
            `).join('');

            document.getElementById('det-total').textContent = moeda(total(pedido));

            modal.classList.add('aberto');
        }

        function fecharModal() {
            modal.classList.remove('aberto');
        }

        corpo.addEventListener('change', e => {
            const select = e.target.closest('.status-select');
            if (!select) return;

            const pedido = pedidos.find(p => p.id === Number(select.dataset.id));
            pedido.status = select.value;
            render();
        });

        corpo.addEventListener('click', e => {
            const btn = e.target.closest('.btn-acao');
            if (!btn) return;

            abrirDetalhes(pedidos.find(p => p.id === Number(btn.dataset.id)));
        });

        document.querySelectorAll('.chip').forEach(chip => {
            chip.addEventListener('click', () => {
                document.querySelectorAll('.chip').forEach(c => c.classList.remove('ativo'));
                chip.classList.add('ativo');
                filtroStatus = chip.dataset.status;
                render();
            });
        });

        document.getElementById('btn-fechar').addEventListener('click', fecharModal);

        modal.addEventListener('click', e => {
            if (e.target === modal) fecharModal();
        });

        document.addEventListener('keydown', e => {
            if (e.key === 'Escape') fecharModal();
        });

        busca.addEventListener('input', render);

        render();
    </script>

</body>

</html>