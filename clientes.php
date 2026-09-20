<!DOCTYPE html>
<html lang="pt-BR">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <title>Clientes — Garimpo Chic</title>
    <link rel="stylesheet" href="./src/css/dashfunc.css">
    <link rel="stylesheet" href="./src/css/produtos.css">
    <link rel="stylesheet" href="./src/css/clientes.css">
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

            <a href="pedidos.php">
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

            <a href="clientes.php" class="ativo">
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
                <h2>Clientes ✦</h2>
                <p>Pessoas cadastradas na loja e seus papéis</p>
            </div>

            <a href="index.php" class="logout">Logout</a>

        </div>

        <!-- BUSCA -->

        <div class="barra-prod">
            <div class="busca">
                <input type="text" id="busca" placeholder="Buscar por nome ou e-mail...">
            </div>
        </div>

        <!-- FILTROS DE PAPEL -->

        <div class="chips">
            <button class="chip ativo" data-papel="todos">Todas</button>
            <button class="chip" data-papel="cliente">Clientes</button>
            <button class="chip" data-papel="vendedora">Vendedoras externas</button>
            <button class="chip" data-papel="funcionaria">Funcionárias</button>
        </div>

        <p class="contador" id="contador"></p>

        <!-- TABELA -->

        <div class="painel">

            <div class="tabela-wrap">
                <table class="tabela">
                    <thead>
                        <tr>
                            <th>Pessoa</th>
                            <th>Papéis</th>
                            <th>Cidade</th>
                            <th>Pedidos</th>
                            <th>Total gasto</th>
                            <th class="col-acoes">Ações</th>
                        </tr>
                    </thead>
                    <tbody id="corpo"></tbody>
                </table>
            </div>

            <p class="vazio" id="vazio">Nenhuma pessoa encontrada ✦</p>

        </div>

    </main>

    <!-- MODAL DE DETALHES -->

    <div class="modal-fundo" id="modal">

        <div class="modal">

            <div class="det-cab">
                <div class="avatar-mini grande" id="det-avatar"></div>
                <div>
                    <h3 id="det-nome"></h3>
                    <div id="det-papeis"></div>
                </div>
            </div>

            <div id="det-linhas"></div>

            <div class="acoes-modal">
                <button type="button" class="btn-secundario" id="btn-fechar">Fechar</button>
            </div>

        </div>

    </div>

    <script>
        const PAPEIS = {
            cliente: 'Cliente',
            vendedora: 'Vendedora externa',
            funcionaria: 'Funcionária'
        };

        const pessoas = [
            {
                nome: 'Julia', email: 'julia@email.com', telefone: '(47) 99999-9999',
                cpf: '•••.982.247-••', cidade: 'Joinville', desde: '02/08/2026',
                papeis: ['cliente', 'funcionaria'], pedidos: 4, gasto: 459.50, pecasEnviadas: 0
            },
            {
                nome: 'Ana Clara', email: 'anaclara@email.com', telefone: '(47) 98888-1234',
                cpf: '•••.444.777-••', cidade: 'Joinville', desde: '10/08/2026',
                papeis: ['cliente', 'vendedora'], pedidos: 2, gasto: 109.80, pecasEnviadas: 12
            },
            {
                nome: 'Mariana Souza', email: 'mariana.souza@email.com', telefone: '(47) 97777-4321',
                cpf: '•••.123.456-••', cidade: 'Jaraguá do Sul', desde: '15/08/2026',
                papeis: ['cliente'], pedidos: 1, gasto: 139.90, pecasEnviadas: 0
            },
            {
                nome: 'Beatriz Lima', email: 'bia.lima@email.com', telefone: '(47) 96666-8765',
                cpf: '•••.321.654-••', cidade: 'Blumenau', desde: '20/07/2026',
                papeis: ['cliente', 'vendedora'], pedidos: 1, gasto: 209.80, pecasEnviadas: 8
            },
            {
                nome: 'Camila Rocha', email: 'camila.rocha@email.com', telefone: '(47) 95555-2468',
                cpf: '•••.789.012-••', cidade: 'São Francisco do Sul', desde: '05/09/2026',
                papeis: ['vendedora'], pedidos: 0, gasto: 0, pecasEnviadas: 5
            }
        ];

        let filtroPapel = 'todos';

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

        function tagsPapeis(papeis) {
            return papeis.map(p => `<span class="papel ${p}">${PAPEIS[p]}</span>`).join('');
        }

        function render() {
            const texto = busca.value.trim().toLowerCase();

            const lista = pessoas.filter(p =>
                (filtroPapel === 'todos' || p.papeis.includes(filtroPapel)) &&
                (p.nome.toLowerCase().includes(texto) || p.email.toLowerCase().includes(texto))
            );

            corpo.innerHTML = lista.map(p => `
                <tr>
                    <td>
                        <div class="produto-cel">
                            <div class="avatar-mini">${esc(p.nome.charAt(0))}</div>
                            <div>
                                <strong>${esc(p.nome)}</strong>
                                <p>${esc(p.email)}</p>
                            </div>
                        </div>
                    </td>
                    <td>${tagsPapeis(p.papeis)}</td>
                    <td>${esc(p.cidade)}</td>
                    <td>${p.pedidos}</td>
                    <td class="preco-cel">${moeda(p.gasto)}</td>
                    <td class="col-acoes">
                        <button class="btn-acao" data-email="${esc(p.email)}">Ver detalhes</button>
                    </td>
                </tr>
            `).join('');

            contador.textContent = lista.length + (lista.length === 1 ? ' pessoa' : ' pessoas');
            vazio.style.display = lista.length === 0 ? 'block' : 'none';
        }

        function abrirDetalhes(p) {
            document.getElementById('det-avatar').textContent = p.nome.charAt(0);
            document.getElementById('det-nome').textContent = p.nome;
            document.getElementById('det-papeis').innerHTML = tagsPapeis(p.papeis);

            const linhas = [
                ['E-mail', p.email],
                ['Telefone', p.telefone],
                ['CPF', p.cpf],
                ['Cidade', p.cidade],
                ['Cadastrada em', p.desde],
                ['Pedidos realizados', p.pedidos],
                ['Total gasto', moeda(p.gasto)]
            ];

            if (p.papeis.includes('vendedora')) {
                linhas.push(['Peças enviadas à loja', p.pecasEnviadas]);
            }

            document.getElementById('det-linhas').innerHTML = linhas.map(l => `
                <div class="det-linha">
                    <span>${l[0]}</span>
                    <strong>${esc(String(l[1]))}</strong>
                </div>
            `).join('');

            modal.classList.add('aberto');
        }

        function fecharModal() {
            modal.classList.remove('aberto');
        }

        corpo.addEventListener('click', e => {
            const btn = e.target.closest('.btn-acao');
            if (!btn) return;

            abrirDetalhes(pessoas.find(p => p.email === btn.dataset.email));
        });

        document.querySelectorAll('.chip').forEach(chip => {
            chip.addEventListener('click', () => {
                document.querySelectorAll('.chip').forEach(c => c.classList.remove('ativo'));
                chip.classList.add('ativo');
                filtroPapel = chip.dataset.papel;
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