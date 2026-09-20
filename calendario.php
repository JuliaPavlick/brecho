<!DOCTYPE html>
<html lang="pt-BR">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <title>Calendário — Garimpo Chic</title>
    <link rel="stylesheet" href="./src/css/dashfunc.css">
    <link rel="stylesheet" href="./src/css/produtos.css">
    <link rel="stylesheet" href="./src/css/calendario.css">
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

            <a href="calendario.php" class="ativo">
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
                <h2>Calendário ✦</h2>
                <p>Organize recebimentos, avaliações e datas da loja</p>
            </div>

            <a href="index.php" class="logout">Logout</a>

        </div>

        <div class="cal-layout">

            <!-- CALENDÁRIO -->

            <div class="painel">

                <div class="cal-cab">

                    <h3 id="cal-titulo"></h3>

                    <div class="cal-nav">
                        <button type="button" class="btn-acao" id="btn-anterior">←</button>
                        <button type="button" class="btn-acao" id="btn-hoje">Hoje</button>
                        <button type="button" class="btn-acao" id="btn-proximo">→</button>
                    </div>

                </div>

                <div class="cal-semana">
                    <span>Dom</span>
                    <span>Seg</span>
                    <span>Ter</span>
                    <span>Qua</span>
                    <span>Qui</span>
                    <span>Sex</span>
                    <span>Sáb</span>
                </div>

                <div class="cal-dias" id="cal-dias"></div>

                <div class="legenda">
                    <span><i class="dot recebimento"></i> Recebimento</span>
                    <span><i class="dot avaliacao"></i> Avaliação</span>
                    <span><i class="dot colecao"></i> Coleção</span>
                    <span><i class="dot outro"></i> Outro</span>
                </div>

            </div>

            <!-- EVENTOS DO DIA -->

            <div class="painel">

                <h3 id="dia-titulo"></h3>

                <div id="dia-eventos"></div>

                <button type="button" class="btn-primario btn-novo-evento" id="btn-novo">+ Novo evento</button>

            </div>

        </div>

    </main>

    <!-- MODAL -->

    <div class="modal-fundo" id="modal">

        <div class="modal">

            <h3>Novo evento</h3>

            <form id="form-evento" novalidate>

                <div class="campo">
                    <label for="titulo">Título</label>
                    <input type="text" id="titulo" placeholder="Ex.: Recebimento de peças">
                    <span class="erro-campo" id="erro-titulo">Informe o título do evento.</span>
                </div>

                <div class="linha-campos">

                    <div class="campo">
                        <label for="data">Data</label>
                        <input type="date" id="data">
                        <span class="erro-campo" id="erro-data">Informe a data.</span>
                    </div>

                    <div class="campo">
                        <label for="hora">Horário</label>
                        <input type="time" id="hora" value="10:00">
                    </div>

                </div>

                <div class="campo">
                    <label for="tipo">Tipo</label>
                    <select id="tipo">
                        <option value="recebimento">Recebimento de peças</option>
                        <option value="avaliacao">Avaliação de peças</option>
                        <option value="colecao">Coleção / destaque</option>
                        <option value="outro">Outro</option>
                    </select>
                </div>

                <div class="acoes-modal">
                    <button type="button" class="btn-secundario" id="btn-cancelar">Cancelar</button>
                    <button type="submit" class="btn-primario">Salvar</button>
                </div>

            </form>

        </div>

    </div>

    <script>
        const MESES = ['janeiro', 'fevereiro', 'março', 'abril', 'maio', 'junho',
            'julho', 'agosto', 'setembro', 'outubro', 'novembro', 'dezembro'];

        const TIPOS = {
            recebimento: 'Recebimento',
            avaliacao: 'Avaliação',
            colecao: 'Coleção',
            outro: 'Outro'
        };

        function chave(d) {
            return d.getFullYear() + '-' +
                String(d.getMonth() + 1).padStart(2, '0') + '-' +
                String(d.getDate()).padStart(2, '0');
        }

        const hoje = new Date();
        const chaveHoje = chave(hoje);

        function daqui(dias) {
            return chave(new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate() + dias));
        }

        let eventos = [
            { id: 1, data: daqui(0), hora: '14:00', titulo: 'Recebimento de peças da Ana Clara', tipo: 'recebimento' },
            { id: 2, data: daqui(1), hora: '10:00', titulo: 'Avaliação de roupas vintage', tipo: 'avaliacao' },
            { id: 3, data: daqui(4), hora: '10:00', titulo: 'Nova coleção em destaque', tipo: 'colecao' },
            { id: 4, data: daqui(9), hora: '09:00', titulo: 'Funcionamento reduzido da loja', tipo: 'outro' }
        ];

        let proximoId = 5;
        let ano = hoje.getFullYear();
        let mes = hoje.getMonth();
        let selecionada = chaveHoje;

        const calDias = document.getElementById('cal-dias');
        const modal = document.getElementById('modal');

        function esc(texto) {
            return texto.replace(/[&<>"']/g, c => ({
                '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
            }[c]));
        }

        function eventosDoDia(dia) {
            return eventos
                .filter(e => e.data === dia)
                .sort((a, b) => a.hora.localeCompare(b.hora));
        }

        function renderCalendario() {
            document.getElementById('cal-titulo').textContent = MESES[mes] + ' de ' + ano;

            const primeiro = new Date(ano, mes, 1).getDay();
            const total = new Date(ano, mes + 1, 0).getDate();

            let html = '';

            for (let i = 0; i < primeiro; i++) {
                html += '<div class="cel-vazia"></div>';
            }

            for (let d = 1; d <= total; d++) {
                const dia = chave(new Date(ano, mes, d));
                const tipos = [...new Set(eventosDoDia(dia).map(e => e.tipo))].slice(0, 4);

                const classes = ['dia'];
                if (dia === chaveHoje) classes.push('hoje');
                if (dia === selecionada) classes.push('selecionado');

                html += `
                    <button type="button" class="${classes.join(' ')}" data-dia="${dia}">
                        <span class="num">${d}</span>
                        <span class="dots">${tipos.map(t => `<i class="dot ${t}"></i>`).join('')}</span>
                    </button>
                `;
            }

            calDias.innerHTML = html;
        }

        function renderDia() {
            const [a, m, d] = selecionada.split('-').map(Number);
            const data = new Date(a, m - 1, d);

            document.getElementById('dia-titulo').textContent =
                data.toLocaleDateString('pt-BR', { weekday: 'long', day: 'numeric', month: 'long' });

            const lista = eventosDoDia(selecionada);
            const area = document.getElementById('dia-eventos');

            if (lista.length === 0) {
                area.innerHTML = '<p class="sem-eventos">Nenhum evento neste dia ✦</p>';
                return;
            }

            area.innerHTML = lista.map(e => `
                <div class="ev-item ${e.tipo}">
                    <div class="ev-info">
                        <strong>${e.hora} • ${esc(e.titulo)}</strong>
                        <p>${TIPOS[e.tipo]}</p>
                    </div>
                    <button type="button" class="btn-acao perigo" data-id="${e.id}">Excluir</button>
                </div>
            `).join('');
        }

        function render() {
            renderCalendario();
            renderDia();
        }

        function abrirModal() {
            document.getElementById('titulo').value = '';
            document.getElementById('data').value = selecionada;
            document.getElementById('hora').value = '10:00';
            document.getElementById('tipo').value = 'recebimento';
            document.getElementById('erro-titulo').style.display = 'none';
            document.getElementById('erro-data').style.display = 'none';
            modal.classList.add('aberto');
        }

        function fecharModal() {
            modal.classList.remove('aberto');
        }

        document.getElementById('btn-anterior').addEventListener('click', () => {
            mes--;
            if (mes < 0) { mes = 11; ano--; }
            renderCalendario();
        });

        document.getElementById('btn-proximo').addEventListener('click', () => {
            mes++;
            if (mes > 11) { mes = 0; ano++; }
            renderCalendario();
        });

        document.getElementById('btn-hoje').addEventListener('click', () => {
            ano = hoje.getFullYear();
            mes = hoje.getMonth();
            selecionada = chaveHoje;
            render();
        });

        calDias.addEventListener('click', e => {
            const btn = e.target.closest('.dia');
            if (!btn) return;

            selecionada = btn.dataset.dia;
            render();
        });

        document.getElementById('dia-eventos').addEventListener('click', e => {
            const btn = e.target.closest('.btn-acao');
            if (!btn) return;

            const evento = eventos.find(ev => ev.id === Number(btn.dataset.id));

            if (confirm('Excluir "' + evento.titulo + '"?')) {
                eventos = eventos.filter(ev => ev.id !== evento.id);
                render();
            }
        });

        document.getElementById('btn-novo').addEventListener('click', abrirModal);
        document.getElementById('btn-cancelar').addEventListener('click', fecharModal);

        modal.addEventListener('click', e => {
            if (e.target === modal) fecharModal();
        });

        document.addEventListener('keydown', e => {
            if (e.key === 'Escape') fecharModal();
        });

        document.getElementById('form-evento').addEventListener('submit', e => {
            e.preventDefault();

            const titulo = document.getElementById('titulo').value.trim();
            const data = document.getElementById('data').value;

            document.getElementById('erro-titulo').style.display = titulo ? 'none' : 'block';
            document.getElementById('erro-data').style.display = data ? 'none' : 'block';

            if (!titulo || !data) return;

            eventos.push({
                id: proximoId++,
                data: data,
                hora: document.getElementById('hora').value || '00:00',
                titulo: titulo,
                tipo: document.getElementById('tipo').value
            });

            const [a, m] = data.split('-').map(Number);
            ano = a;
            mes = m - 1;
            selecionada = data;

            fecharModal();
            render();
        });

        render();
    </script>

</body>

</html>