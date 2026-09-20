<!DOCTYPE html>
<html lang="pt-BR">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <title>Produtos — Garimpo Chic</title>
    <link rel="stylesheet" href="./src/css/dashfunc.css">
    <link rel="stylesheet" href="./src/css/produtos.css">
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

            <a href="produtos.php" class="ativo">
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
                <h2>Produtos ✦</h2>
                <p>Gerencie as peças da loja</p>
            </div>

            <a href="index.php" class="logout">Logout</a>

        </div>

        <!-- BUSCA + NOVO -->

        <div class="barra-prod">

            <div class="busca">
                <input type="text" id="busca" placeholder="Buscar peça pelo nome...">
            </div>

            <button type="button" class="btn-primario" id="btn-novo">+ Novo produto</button>

        </div>

        <!-- FILTROS DE STATUS -->

        <div class="chips">
            <button class="chip ativo" data-status="todos">Todos</button>
            <button class="chip" data-status="disponivel">Disponível</button>
            <button class="chip" data-status="reservado">Reservado</button>
            <button class="chip" data-status="avaliacao">Em avaliação</button>
            <button class="chip" data-status="vendido">Vendido</button>
        </div>

        <p class="contador" id="contador"></p>

        <!-- TABELA -->

        <div class="painel">

            <div class="tabela-wrap">
                <table class="tabela">
                    <thead>
                        <tr>
                            <th>Peça</th>
                            <th>Categoria</th>
                            <th>Preço</th>
                            <th>Status</th>
                            <th class="col-acoes">Ações</th>
                        </tr>
                    </thead>
                    <tbody id="corpo"></tbody>
                </table>
            </div>

            <p class="vazio" id="vazio">Nenhuma peça encontrada ✦</p>

        </div>

    </main>

    <!-- MODAL -->

    <div class="modal-fundo" id="modal">

        <div class="modal">

            <h3 id="modal-titulo">Novo produto</h3>

            <form id="form-produto" novalidate>

                <div class="campo">
                    <label for="nome">Nome</label>
                    <input type="text" id="nome" placeholder="Ex.: Blusa Vintage">
                    <span class="erro-campo" id="erro-nome">Informe o nome da peça.</span>
                </div>

                <div class="campo">
                    <label for="descricao">Descrição</label>
                    <textarea id="descricao" rows="3" placeholder="Tamanho, estado de conservação, detalhes..."></textarea>
                </div>

                <div class="linha-campos">

                    <div class="campo">
                        <label for="categoria">Categoria</label>
                        <select id="categoria">
                            <option>Roupas</option>
                            <option>Vestidos</option>
                            <option>Bolsas</option>
                            <option>Calçados</option>
                            <option>Acessórios</option>
                        </select>
                    </div>

                    <div class="campo">
                        <label for="preco">Preço (R$)</label>
                        <input type="number" id="preco" step="0.01" min="0" placeholder="0,00">
                        <span class="erro-campo" id="erro-preco">Informe um preço maior que zero.</span>
                    </div>

                </div>

                <div class="campo">
                    <label for="status">Status</label>
                    <select id="status">
                        <option value="avaliacao">Em avaliação</option>
                        <option value="disponivel">Disponível</option>
                        <option value="reservado">Reservado</option>
                        <option value="vendido">Vendido</option>
                    </select>
                </div>

                <div class="campo">
                    <label for="fotos">Fotos</label>
                    <input type="file" id="fotos" accept="image/*" multiple>
                </div>

                <div class="acoes-modal">
                    <button type="button" class="btn-secundario" id="btn-cancelar">Cancelar</button>
                    <button type="submit" class="btn-primario">Salvar</button>
                </div>

            </form>

        </div>

    </div>

    <script>
        const EMOJIS = {
            'Roupas': '👚',
            'Vestidos': '👗',
            'Bolsas': '👜',
            'Calçados': '👠',
            'Acessórios': '👒'
        };

        const STATUS = {
            disponivel: 'Disponível',
            reservado: 'Reservado',
            avaliacao: 'Em avaliação',
            vendido: 'Vendido'
        };

        let produtos = [
            { id: 1, nome: 'Blusa Vintage', descricao: 'Tamanho M, ótimo estado', categoria: 'Roupas', preco: 79.90, status: 'disponivel' },
            { id: 2, nome: 'Vestido Midi Floral', descricao: 'Tamanho M, sem defeitos', categoria: 'Vestidos', preco: 139.90, status: 'disponivel' },
            { id: 3, nome: 'Bolsa Retrô', descricao: 'Couro sintético, alça ajustável', categoria: 'Bolsas', preco: 129.90, status: 'reservado' },
            { id: 4, nome: 'Salto Clássico', descricao: 'Número 37, última unidade', categoria: 'Calçados', preco: 159.90, status: 'disponivel' },
            { id: 5, nome: 'Casaco de Lã', descricao: 'Tamanho G, veio de vendedora externa', categoria: 'Roupas', preco: 189.90, status: 'avaliacao' },
            { id: 6, nome: 'Óculos Vintage', descricao: 'Anos 80, armação original', categoria: 'Acessórios', preco: 59.90, status: 'vendido' }
        ];

        let proximoId = 7;
        let editandoId = null;
        let filtroStatus = 'todos';

        const corpo = document.getElementById('corpo');
        const busca = document.getElementById('busca');
        const contador = document.getElementById('contador');
        const vazio = document.getElementById('vazio');
        const modal = document.getElementById('modal');
        const form = document.getElementById('form-produto');

        function esc(texto) {
            return texto.replace(/[&<>"']/g, c => ({
                '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
            }[c]));
        }

        function moeda(valor) {
            return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
        }

        function render() {
            const texto = busca.value.trim().toLowerCase();

            const lista = produtos.filter(p =>
                (filtroStatus === 'todos' || p.status === filtroStatus) &&
                p.nome.toLowerCase().includes(texto)
            );

            corpo.innerHTML = lista.map(p => `
                <tr>
                    <td>
                        <div class="produto-cel">
                            <div class="produto-thumb">${EMOJIS[p.categoria]}</div>
                            <div>
                                <strong>${esc(p.nome)}</strong>
                                <p>${esc(p.descricao)}</p>
                            </div>
                        </div>
                    </td>
                    <td>${p.categoria}</td>
                    <td class="preco-cel">${moeda(p.preco)}</td>
                    <td><span class="badge ${p.status}">${STATUS[p.status]}</span></td>
                    <td class="col-acoes">
                        <button class="btn-acao" data-acao="editar" data-id="${p.id}">Editar</button>
                        <button class="btn-acao perigo" data-acao="excluir" data-id="${p.id}">Excluir</button>
                    </td>
                </tr>
            `).join('');

            contador.textContent = lista.length + (lista.length === 1 ? ' peça' : ' peças');
            vazio.style.display = lista.length === 0 ? 'block' : 'none';
        }

        function abrirModal(produto) {
            editandoId = produto ? produto.id : null;

            document.getElementById('modal-titulo').textContent = produto ? 'Editar produto' : 'Novo produto';
            document.getElementById('nome').value = produto ? produto.nome : '';
            document.getElementById('descricao').value = produto ? produto.descricao : '';
            document.getElementById('categoria').value = produto ? produto.categoria : 'Roupas';
            document.getElementById('preco').value = produto ? produto.preco : '';
            document.getElementById('status').value = produto ? produto.status : 'avaliacao';
            document.getElementById('fotos').value = '';

            document.getElementById('erro-nome').style.display = 'none';
            document.getElementById('erro-preco').style.display = 'none';

            modal.classList.add('aberto');
        }

        function fecharModal() {
            modal.classList.remove('aberto');
        }

        document.getElementById('btn-novo').addEventListener('click', () => abrirModal(null));
        document.getElementById('btn-cancelar').addEventListener('click', fecharModal);

        modal.addEventListener('click', e => {
            if (e.target === modal) fecharModal();
        });

        document.addEventListener('keydown', e => {
            if (e.key === 'Escape') fecharModal();
        });

        form.addEventListener('submit', e => {
            e.preventDefault();

            const nome = document.getElementById('nome').value.trim();
            const preco = parseFloat(document.getElementById('preco').value);

            const okNome = nome !== '';
            const okPreco = preco > 0;

            document.getElementById('erro-nome').style.display = okNome ? 'none' : 'block';
            document.getElementById('erro-preco').style.display = okPreco ? 'none' : 'block';

            if (!okNome || !okPreco) return;

            const dados = {
                nome: nome,
                descricao: document.getElementById('descricao').value.trim(),
                categoria: document.getElementById('categoria').value,
                preco: preco,
                status: document.getElementById('status').value
            };

            if (editandoId) {
                const p = produtos.find(item => item.id === editandoId);
                Object.assign(p, dados);
            } else {
                produtos.unshift({ id: proximoId++, ...dados });
            }

            fecharModal();
            render();
        });

        corpo.addEventListener('click', e => {
            const btn = e.target.closest('.btn-acao');
            if (!btn) return;

            const id = Number(btn.dataset.id);
            const produto = produtos.find(p => p.id === id);

            if (btn.dataset.acao === 'editar') {
                abrirModal(produto);
            } else if (confirm('Excluir "' + produto.nome + '"?')) {
                produtos = produtos.filter(p => p.id !== id);
                render();
            }
        });

        document.querySelectorAll('.chip').forEach(chip => {
            chip.addEventListener('click', () => {
                document.querySelectorAll('.chip').forEach(c => c.classList.remove('ativo'));
                chip.classList.add('ativo');
                filtroStatus = chip.dataset.status;
                render();
            });
        });

        busca.addEventListener('input', render);

        render();
    </script>

</body>

</html>