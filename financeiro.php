<!DOCTYPE html>
<html lang="pt-BR">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <title>Financeiro — Garimpo Chic</title>
    <link rel="stylesheet" href="./src/css/dashfunc.css">
    <link rel="stylesheet" href="./src/css/produtos.css">
    <link rel="stylesheet" href="./src/css/financeiro.css">
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

            <a href="financeiro.php" class="ativo">
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
                <h2>Financeiro ✦</h2>
                <p>Vendas, repasses e resultado da loja</p>
            </div>

            <a href="index.php" class="logout">Logout</a>

        </div>

        <!-- CARDS -->

        <section class="cards">

            <div class="card">
                <div class="card-topo">
                    <span class="card-titulo">Receita do mês</span>
                    <span class="card-icone">
                        <img src="./src/img/sacola-dinheiro.png" alt="Receita">
                    </span>
                </div>
                <h3 class="card-valor">R$ 18.400</h3>
                <p class="card-info">Total de vendas em setembro</p>
            </div>

            <div class="card">
                <div class="card-topo">
                    <span class="card-titulo">Repasses</span>
                    <span class="card-icone">
                        <img src="./src/img/pessoas.png" alt="Repasses">
                    </span>
                </div>
                <h3 class="card-valor">R$ 6.400</h3>
                <p class="card-info">Devido às vendedoras externas</p>
            </div>

            <div class="card">
                <div class="card-topo">
                    <span class="card-titulo">Lucro bruto</span>
                    <span class="card-icone">
                        <img src="./src/img/grafico-de-crescimento.png" alt="Lucro">
                    </span>
                </div>
                <h3 class="card-valor">R$ 12.000</h3>
                <p class="card-info">Receita menos repasses</p>
            </div>

            <div class="card">
                <div class="card-topo">
                    <span class="card-titulo">Ticket médio</span>
                    <span class="card-icone">
                        <img src="./src/img/sacolas-de-compras.png" alt="Ticket médio">
                    </span>
                </div>
                <h3 class="card-valor">R$ 129,58</h3>
                <p class="card-info">Valor médio por peça vendida</p>
            </div>

        </section>

        <!-- GRAFICO -->
<!-- Biblioteca Chart.js (inclua no <head> ou antes deste bloco) -->
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

<!-- Bloco do Painel com o Gráfico -->
<div class="painel" style="background: #FFFDFB; border-radius: 16px; padding: 24px; box-shadow: 0 4px 20px rgba(216, 125, 154, 0.08); border: 1px solid #F5EAE6;">
    <div class="painel-cabecalho" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px;">
        <div>
            <h3 style="font-family: 'Playfair Display', serif; color: #4A3A3E; margin: 0; font-size: 1.25rem;">Movimento de Vendas ✦</h3>
            <p style="font-family: 'Lato', sans-serif; color: #8C7A80; font-size: 0.85rem; margin: 4px 0 0 0;">Acompanhamento do desempenho financeiro</p>
        </div>
        
        <!-- Filtros de Período -->
        <div class="filtros" style="display: flex; gap: 8px; background: #FDF7F5; padding: 4px; border-radius: 24px; border: 1px solid #F3E5E1;">
            <button type="button" class="btn-filtro ativo" onclick="atualizarGrafico('semana', this)">Semana</button>
            <button type="button" class="btn-filtro" onclick="atualizarGrafico('mes', this)">Mês</button>
            <button type="button" class="btn-filtro" onclick="atualizarGrafico('ano', this)">Ano</button>
        </div>
    </div>

    <!-- Contêiner do Gráfico Canvas -->
    <div style="position: relative; height: 320px; width: 100%;">
        <canvas id="graficoRosaChampanhe"></canvas>
    </div>
</div>

<script>
    // Dados para alternar no gráfico
    const DADOS_GRAFICO = {
        semana: {
            labels: ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom'],
            valores: [1240, 2180, 3020, 1870, 3880, 2760, 980]
        },
        mes: {
            labels: ['Semana 1', 'Semana 2', 'Semana 3', 'Semana 4'],
            valores: [3900, 4600, 4300, 5600]
        },
        ano: {
            labels: ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set'],
            valores: [9800, 8600, 11200, 12400, 13900, 12800, 15200, 16700, 18400]
        }
    };

    const ctx = document.getElementById('graficoRosaChampanhe').getContext('2d');

    // Gradiente suave no tom Rosa / Champanhe
    const gradiente = ctx.createLinearGradient(0, 0, 0, 300);
    gradiente.addColorStop(0, 'rgba(216, 125, 154, 0.35)'); // Rosa translúcido no topo
    gradiente.addColorStop(1, 'rgba(247, 235, 228, 0.05)'); // Champanhe suave na base

    let chartInstancia = new Chart(ctx, {
        type: 'line',
        data: {
            labels: DADOS_GRAFICO.semana.labels,
            datasets: [{
                label: 'Vendas (R$)',
                data: DADOS_GRAFICO.semana.valores,
                borderColor: '#D87D9A',             // Rosa Principal
                borderWidth: 3,
                backgroundColor: gradiente,         // Gradiente Rosa & Champanhe
                fill: true,
                tension: 0.4,                       // Curvatura elegante na linha
                pointBackgroundColor: '#D87D9A',
                pointBorderColor: '#FFFFFF',
                pointBorderWidth: 2,
                pointRadius: 5,
                pointHoverRadius: 8,
                pointHoverBackgroundColor: '#B85D7A',
                pointHoverBorderColor: '#FFFFFF'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: '#4A3A3E',       // Tom escuro de vinho/marrom para contraste
                    titleFont: { family: 'Lato', size: 12 },
                    bodyFont: { family: 'Lato', size: 14, weight: 'bold' },
                    padding: 12,
                    cornerRadius: 10,
                    displayColors: false,
                    callbacks: {
                        label: function(context) {
                            return 'R$ ' + context.parsed.y.toLocaleString('pt-BR', { minimumFractionDigits: 2 });
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: { font: { family: 'Lato', size: 12 }, color: '#8C7A80' }
                },
                y: {
                    border: { dash: [4, 4] },
                    grid: { color: '#F5EAE6' },      // Linhas de grade sutis tom Champanhe
                    ticks: {
                        font: { family: 'Lato', size: 12 },
                        color: '#8C7A80',
                        callback: function(value) { return 'R$ ' + value; }
                    }
                }
            }
        }
    });

    function atualizarGrafico(periodo, el) {
        document.querySelectorAll('.btn-filtro').forEach(btn => btn.classList.remove('ativo'));
        el.classList.add('ativo');

        chartInstancia.data.labels = DADOS_GRAFICO[periodo].labels;
        chartInstancia.data.datasets[0].data = DADOS_GRAFICO[periodo].valores;
        chartInstancia.update();
    }
</script>

        <!-- MOVIMENTACOES -->

        <div class="painel painel-tabela">

            <div class="cab-tabela">
                <h3>Últimas movimentações</h3>

                <div class="chips">
                    <button class="chip ativo" data-tipo="todos">Todas</button>
                    <button class="chip" data-tipo="venda">Vendas</button>
                    <button class="chip" data-tipo="repasse">Repasses</button>
                </div>
            </div>

            <div class="tabela-wrap">
                <table class="tabela">
                    <thead>
                        <tr>
                            <th>Data</th>
                            <th>Descrição</th>
                            <th>Tipo</th>
                            <th class="col-valor">Valor</th>
                        </tr>
                    </thead>
                    <tbody id="corpo"></tbody>
                </table>
            </div>

            <p class="vazio" id="vazio">Nenhuma movimentação encontrada ✦</p>

        </div>

    </main>

    <script>
        const PERIODOS = {
            semana: [
                ['Seg', 1240], ['Ter', 2180], ['Qua', 3020], ['Qui', 1870],
                ['Sex', 3880], ['Sáb', 2760], ['Dom', 980]
            ],
            mes: [
                ['Sem 1', 3900], ['Sem 2', 4600], ['Sem 3', 4300], ['Sem 4', 5600]
            ],
            ano: [
                ['Jan', 9800], ['Fev', 8600], ['Mar', 11200], ['Abr', 12400], ['Mai', 13900],
                ['Jun', 12800], ['Jul', 15200], ['Ago', 16700], ['Set', 18400]
            ]
        };

        const movimentacoes = [
            { data: '20/09/2026', descricao: 'Venda — Vestido Midi Floral', tipo: 'venda', valor: 139.90 },
            { data: '19/09/2026', descricao: 'Repasse — Ana Clara', tipo: 'repasse', valor: -84.00 },
            { data: '18/09/2026', descricao: 'Venda — Casaco de Lã', tipo: 'venda', valor: 189.90 },
            { data: '17/09/2026', descricao: 'Venda — Bolsa Retrô', tipo: 'venda', valor: 129.90 },
            { data: '15/09/2026', descricao: 'Repasse — Beatriz Lima', tipo: 'repasse', valor: -120.00 },
            { data: '12/09/2026', descricao: 'Venda — Óculos Vintage e Chapéu Palha', tipo: 'venda', valor: 109.80 }
        ];

        const TIPOS = {
            venda: 'Venda',
            repasse: 'Repasse'
        };

        function moeda(valor) {
            return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
        }

        /* GRAFICO */

        const grafico = document.getElementById('grafico');

        function renderGrafico(periodo) {
            const dados = PERIODOS[periodo];
            const maior = Math.max(...dados.map(d => d[1]));

            grafico.innerHTML = dados.map(d => `
                <div class="barra" style="height:${(d[1] / maior) * 92}%;">
                    <div class="tooltip">${moeda(d[1])}</div>
                    <span>${d[0]}</span>
                </div>
            `).join('');
        }

        document.querySelectorAll('.filtro').forEach(filtro => {
            filtro.addEventListener('click', () => {
                document.querySelectorAll('.filtro').forEach(f => f.classList.remove('ativo'));
                filtro.classList.add('ativo');
                renderGrafico(filtro.dataset.periodo);
            });
        });

        /* TABELA */

        const corpo = document.getElementById('corpo');
        const vazio = document.getElementById('vazio');

        function renderTabela(tipo) {
            const lista = movimentacoes.filter(m => tipo === 'todos' || m.tipo === tipo);

            corpo.innerHTML = lista.map(m => `
                <tr>
                    <td>${m.data}</td>
                    <td>${m.descricao}</td>
                    <td><span class="badge ${m.tipo === 'venda' ? 'disponivel' : 'avaliacao'}">${TIPOS[m.tipo]}</span></td>
                    <td class="col-valor ${m.valor > 0 ? 'valor-entrada' : 'valor-saida'}">
                        ${m.valor > 0 ? '+ ' : '− '}${moeda(Math.abs(m.valor))}
                    </td>
                </tr>
            `).join('');

            vazio.style.display = lista.length === 0 ? 'block' : 'none';
        }

        document.querySelectorAll('.chip').forEach(chip => {
            chip.addEventListener('click', () => {
                document.querySelectorAll('.chip').forEach(c => c.classList.remove('ativo'));
                chip.classList.add('ativo');
                renderTabela(chip.dataset.tipo);
            });
        });

        renderGrafico('semana');
        renderTabela('todos');
    </script>

</body>

</html>