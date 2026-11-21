// "Carregando…": enquanto uma ação está em andamento, uma camada cobre a tela e segura os cliques,
// para ninguém disparar o mesmo comando duas vezes (ex.: finalizar a compra, publicar uma peça).
// Conta as ações em andamento: a camada só some quando todas terminam.
const Carregando = (() => {
    let pendentes = 0;
    let camada = null;

    function criar() {
        camada = document.createElement('div');
        camada.className = 'carregando';
        camada.setAttribute('role', 'status');
        camada.setAttribute('aria-live', 'polite');
        camada.innerHTML = '<span class="carregando-aviso"><span class="carregando-roda" aria-hidden="true"></span>Carregando…</span>';
        document.body.appendChild(camada);
    }

    return {
        inicio() {
            pendentes++;
            if (!camada) criar();
            camada.classList.add('ativo');
            document.body.setAttribute('aria-busy', 'true');
        },
        fim() {
            pendentes = Math.max(0, pendentes - 1);
            if (pendentes > 0 || !camada) return;
            camada.classList.remove('ativo');
            document.body.removeAttribute('aria-busy');
        },
        zerar() {
            pendentes = 1;
            this.fim();
        },
    };
})();

// Envia um POST para o servidor com o token anti-falsificação.
// corpo pode ser um FormData (formulário com arquivos) ou um objeto (vira JSON).
function postar(url, corpo) {
    const opcoes = {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('meta[name="csrf"]').content,
            // Faz o servidor responder 401 (em vez de mandar para a tela de login) quando a sessão venceu
            'X-Requested-With': 'XMLHttpRequest'
        }
    };

    if (corpo instanceof FormData) {
        opcoes.body = corpo;
    } else if (corpo !== undefined) {
        opcoes.headers['Content-Type'] = 'application/json';
        opcoes.body = JSON.stringify(corpo);
    }

    Carregando.inicio();
    return fetch(url, opcoes).then(async r => {
        // Sessão vencida: leva para o login e volta para esta página depois de entrar
        if (r.status === 401) {
            location.href = '/entrar?returnUrl=' + encodeURIComponent(location.pathname + location.search);
            return new Promise(() => {});
        }
        if (!r.ok) {
            // Quando o servidor explica o motivo ({ erro: "..." }), a mensagem vai junto no erro
            const corpo = await r.json().catch(() => ({}));
            const erro = new Error(corpo.erro || 'Erro ' + r.status);
            erro.doServidor = !!corpo.erro;
            throw erro;
        }
        return (r.headers.get('content-type') || '').includes('json') ? r.json() : null;
    }).finally(() => Carregando.fim());
}

function falhou(erro) {
    alert(erro && erro.doServidor ? erro.message : 'Não foi possível salvar. Verifique sua conexão e tente novamente.');
}

function esc(texto) {
    return String(texto ?? '').replace(/[&<>"']/g, c => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[c]));
}

function moeda(valor) {
    return Number(valor).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function mascaraCpf(campo) {
    campo.addEventListener('input', () => {
        campo.value = campo.value.replace(/\D/g, '').slice(0, 11)
            .replace(/(\d{3})(\d)/, '$1.$2')
            .replace(/(\d{3})(\d)/, '$1.$2')
            .replace(/(\d{3})(\d{1,2})$/, '$1-$2');
    });
}

function mascaraTelefone(campo) {
    campo.addEventListener('input', () => {
        campo.value = campo.value.replace(/\D/g, '').slice(0, 11)
            .replace(/^(\d{2})(\d)/, '($1) $2')
            .replace(/(\d{5})(\d{1,4})$/, '$1-$2');
    });
}

function cpfValido(valor) {
    const n = valor.replace(/\D/g, '');
    if (n.length !== 11 || /^(\d)\1+$/.test(n)) return false;
    for (let t = 9; t < 11; t++) {
        let soma = 0;
        for (let i = 0; i < t; i++) soma += n[i] * (t + 1 - i);
        if (((soma * 10) % 11) % 10 != n[t]) return false;
    }
    return true;
}

// Mostra ou esconde a mensagem de erro de um campo e liga a mensagem ao campo,
// para o leitor de tela dizer o erro quando o foco chega nele
function mostrarErro(id, ok) {
    const msg = document.getElementById(id);
    msg.style.display = ok ? 'none' : 'block';
    ligarErro(msg, !ok);
    return ok;
}

function ligarErro(msg, comErro) {
    const campo = msg.closest('.campo')?.querySelector('input:not([type="hidden"]), select, textarea')
        || (msg.previousElementSibling?.querySelector?.('input'));
    if (!campo) return;

    const ids = (campo.getAttribute('aria-describedby') || '').split(' ').filter(i => i && i !== msg.id);
    if (comErro) ids.push(msg.id);
    if (ids.length) campo.setAttribute('aria-describedby', ids.join(' '));
    else campo.removeAttribute('aria-describedby');

    if (comErro) campo.setAttribute('aria-invalid', 'true');
    else campo.removeAttribute('aria-invalid');
}

// Erros que já vêm do servidor (ex.: cadastro com CPF repetido): liga ao campo e leva o foco até o primeiro
document.addEventListener('DOMContentLoaded', () => {
    const visiveis = [...document.querySelectorAll('.erro-campo[id]')].filter(m => getComputedStyle(m).display !== 'none');
    visiveis.forEach(m => ligarErro(m, true));
    document.querySelector('[aria-invalid="true"]')?.focus();
});

// Botão "Adicionar à sacola" dos cards (vale para todas as páginas da área do cliente)
document.addEventListener('click', e => {
    // Só os botões de peça (com data-id); o "Finalizar compra" da sacola é um envio de formulário comum
    const btn = e.target.closest('button.btn-sacola[data-id]');
    if (!btn) return;

    btn.disabled = true;
    postar('/sacola/' + btn.dataset.id)
        .then(r => {
            btn.classList.toggle('ativo', r.naSacola);
            btn.textContent = r.naSacola ? 'Na sacola ✓' : 'Adicionar à sacola';
            document.querySelectorAll('.link-sacola').forEach(a => a.hidden = !r.naSacola);

            const qtd = document.getElementById('qtd-sacola');
            if (qtd) {
                qtd.textContent = r.total;
                qtd.style.display = r.total ? '' : 'none';
            }
        })
        .catch(() => alert('Esta peça não está mais disponível.'))
        .finally(() => btn.disabled = false);
});

// Coração dos cards de peça: marca/desmarca no servidor
function alternarFavorito(btn) {
    return postar('/favoritos/' + btn.dataset.id).then(r => {
        btn.classList.toggle('ativo', r.favorito);
        btn.textContent = r.favorito ? '♥' : '♡';
        btn.title = r.favorito ? 'Remover dos favoritos' : 'Adicionar aos favoritos';
        btn.setAttribute('aria-pressed', r.favorito);
        return r.favorito;
    });
}

// Links "Sair": saem por POST com o token, para que um link de outro site não consiga deslogar ninguém
document.addEventListener('click', e => {
    const link = e.target.closest('a[href="/sair"]');
    if (!link) return;
    e.preventDefault();

    const form = document.createElement('form');
    form.method = 'post';
    form.action = '/sair';
    const token = document.createElement('input');
    token.type = 'hidden';
    token.name = '__RequestVerificationToken';
    token.value = document.querySelector('meta[name="csrf"]').content;
    form.appendChild(token);
    document.body.appendChild(form);
    form.submit();
});

// Comportamentos que antes ficavam em onclick/onsubmit no HTML (a CSP bloqueia esse tipo de código)
// <button data-href="/explorar"> navega; <form data-confirmar="Tem certeza?"> pergunta antes de enviar
document.addEventListener('click', e => {
    const alvo = e.target.closest('[data-href]');
    if (alvo) location.href = alvo.dataset.href;
});

document.addEventListener('submit', e => {
    const form = e.target.closest('form[data-confirmar]');
    if (form && !confirm(form.dataset.confirmar)) e.preventDefault();
});

// No celular, cada linha das tabelas do painel vira um cartão (veja site.css).
// Cada célula recebe o nome da sua coluna para aparecer como rótulo dentro do cartão.
function rotularTabela(tabela) {
    const titulos = [...tabela.querySelectorAll('thead th')].map(th => th.textContent.trim());
    tabela.querySelectorAll('tbody tr').forEach(tr =>
        [...tr.children].forEach((td, i) => {
            if (titulos[i] && !td.dataset.rotulo) td.dataset.rotulo = titulos[i];
        }));
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('table.tabela').forEach(tabela => {
        rotularTabela(tabela);
        // As linhas são montadas por JavaScript depois que a página abre
        new MutationObserver(() => rotularTabela(tabela)).observe(tabela, { childList: true, subtree: true });
    });
});

// Fotos tiradas no celular têm vários MB. Antes de enviar, reduz para no máximo 1600 px no lado maior,
// em JPEG, o que deixa o envio e as páginas bem mais leves. Vale para <input type="file" data-reduzir>.
const LADO_MAXIMO = 1600;

async function reduzirFoto(arquivo) {
    if (!arquivo.type.startsWith('image/')) return arquivo;

    let imagem;
    try {
        imagem = await createImageBitmap(arquivo, { imageOrientation: 'from-image' });
    } catch {
        return arquivo; // formato que o navegador não abre: o servidor decide
    }

    const escala = Math.min(1, LADO_MAXIMO / Math.max(imagem.width, imagem.height));
    if (escala === 1 && arquivo.size < 1024 * 1024) return arquivo;

    const canvas = document.createElement('canvas');
    canvas.width = Math.round(imagem.width * escala);
    canvas.height = Math.round(imagem.height * escala);
    const ctx = canvas.getContext('2d');
    ctx.fillStyle = '#fff'; // PNG transparente vira fundo branco no JPEG
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.drawImage(imagem, 0, 0, canvas.width, canvas.height);

    const blob = await new Promise(ok => canvas.toBlob(ok, 'image/jpeg', 0.82));
    if (!blob || blob.size >= arquivo.size) return arquivo;

    const nome = arquivo.name.replace(/\.[^.]+$/, '') + '.jpg';
    return new File([blob], nome, { type: 'image/jpeg' });
}

document.addEventListener('change', async e => {
    const campo = e.target;
    if (!campo.matches('input[type="file"][data-reduzir]') || !campo.files.length) return;

    // Enquanto reduz, o formulário não pode ser enviado com as fotos originais
    const botoes = campo.form ? campo.form.querySelectorAll('button[type="submit"], button:not([type])') : [];
    botoes.forEach(b => b.disabled = true);

    try {
        const reduzidas = await Promise.all([...campo.files].map(reduzirFoto));
        const lista = new DataTransfer();
        reduzidas.forEach(f => lista.items.add(f));
        campo.files = lista.files;
        avisarFotos(campo, reduzidas);
    } finally {
        botoes.forEach(b => b.disabled = false);
    }
});

// As mesmas regras do servidor (Util/Fotos.cs): avisa antes do envio, em vez de a foto sumir sem explicação
const FOTOS_MAX = 5;
const FOTO_MAX_BYTES = 5 * 1024 * 1024;
const FOTO_TIPOS = ['image/jpeg', 'image/png', 'image/webp'];

function avisarFotos(campo, arquivos) {
    const problemas = [];
    const ruins = arquivos.filter(f => !FOTO_TIPOS.includes(f.type)).length;
    const grandes = arquivos.filter(f => FOTO_TIPOS.includes(f.type) && f.size > FOTO_MAX_BYTES).length;
    if (ruins) problemas.push(`${ruins} ${ruins === 1 ? 'arquivo não é' : 'arquivos não são'} JPG, PNG ou WebP`);
    if (grandes) problemas.push(`${grandes} ${grandes === 1 ? 'foto passa' : 'fotos passam'} de 5 MB`);
    if (arquivos.length > FOTOS_MAX) problemas.push(`só as ${FOTOS_MAX} primeiras fotos serão usadas`);

    let aviso = campo.parentElement.querySelector('.aviso-fotos');
    if (!problemas.length) {
        if (aviso) aviso.remove();
        return;
    }
    if (!aviso) {
        aviso = document.createElement('p');
        aviso.className = 'aviso-fotos';
        aviso.setAttribute('role', 'alert');
        campo.insertAdjacentElement('afterend', aviso);
    }
    aviso.textContent = 'Atenção: ' + problemas.join('; ') + '. Essas fotos não serão enviadas.';
}

// Formulário enviado do jeito normal (a página vai recarregar): mostra "Carregando…" e ignora novos envios.
// Fica por último: as validações e o data-confirmar acima já rodaram e podem ter cancelado o envio.
document.addEventListener('submit', e => {
    const form = e.target;
    if (e.defaultPrevented) {
        // A validação barrou o envio: leva o foco ao primeiro campo com erro
        form.querySelector('[aria-invalid="true"]')?.focus();
        return;
    }

    if (form.dataset.enviando) {
        e.preventDefault();
        return;
    }
    form.dataset.enviando = '1';
    Carregando.inicio();
});

// Voltar pelo navegador pode trazer a página "congelada" com o aviso ainda na tela
window.addEventListener('pageshow', () => {
    Carregando.zerar();
    document.querySelectorAll('form[data-enviando]').forEach(f => delete f.dataset.enviando);
});

// Janelas do painel (.modal-fundo que recebe a classe "aberto"): para quem usa teclado ou leitor de tela,
// o foco entra na janela ao abrir, o Tab fica preso dentro dela e, ao fechar, volta ao botão que a abriu
document.addEventListener('DOMContentLoaded', () => {
    const FOCAVEIS = 'a[href], button:not([disabled]), input:not([disabled]):not([type="hidden"]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

    document.querySelectorAll('.modal-fundo').forEach(fundo => {
        const janela = fundo.querySelector('.modal') || fundo;
        const titulo = janela.querySelector('h2, h3');
        janela.setAttribute('role', 'dialog');
        janela.setAttribute('aria-modal', 'true');
        janela.tabIndex = -1;
        if (titulo) {
            titulo.id ||= 'titulo-' + (fundo.id || 'janela');
            janela.setAttribute('aria-labelledby', titulo.id);
        }

        let origem = null;
        new MutationObserver(() => {
            const aberta = fundo.classList.contains('aberto');
            if (aberta && !origem) {
                origem = document.activeElement;
                const primeiro = janela.querySelector(FOCAVEIS);
                (primeiro || janela).focus();
            } else if (!aberta && origem) {
                if (document.contains(origem)) origem.focus();
                origem = null;
            }
        }).observe(fundo, { attributes: true, attributeFilter: ['class'] });

        fundo.addEventListener('keydown', e => {
            if (e.key !== 'Tab') return;
            const itens = [...janela.querySelectorAll(FOCAVEIS)].filter(el => el.offsetParent !== null);
            if (!itens.length) return;
            const primeiro = itens[0], ultimo = itens[itens.length - 1];
            if (e.shiftKey && document.activeElement === primeiro) { e.preventDefault(); ultimo.focus(); }
            else if (!e.shiftKey && document.activeElement === ultimo) { e.preventDefault(); primeiro.focus(); }
        });
    });
});

// Celular: a barra de baixo só cabe 4 ou 5 itens. Os itens marcados com "extra" somem da barra
// e aparecem num painel aberto pelo botão "Mais" (no computador, o botão e o painel ficam escondidos).
document.addEventListener('DOMContentLoaded', () => {
    const botao = document.querySelector('.menu-mais');
    if (!botao) return;

    const extras = [...document.querySelectorAll('.sidebar .extra')];
    const painel = document.createElement('nav');
    painel.className = 'painel-mais';
    painel.id = 'painel-mais';
    painel.setAttribute('aria-label', 'Mais opções');
    painel.hidden = true;
    extras.forEach(item => painel.appendChild(item.cloneNode(true)));
    document.body.appendChild(painel);

    // A página aberta está escondida no "Mais": o botão fica destacado
    if (extras.some(item => item.classList.contains('ativo'))) botao.classList.add('ativo');

    function abrir(aberto) {
        painel.hidden = !aberto;
        botao.setAttribute('aria-expanded', aberto);
        if (aberto) painel.querySelector('a')?.focus();
    }

    botao.addEventListener('click', () => abrir(painel.hidden));
    document.addEventListener('click', e => {
        if (!painel.hidden && !painel.contains(e.target) && !botao.contains(e.target)) abrir(false);
    });
    document.addEventListener('keydown', e => {
        if (e.key === 'Escape' && !painel.hidden) {
            abrir(false);
            botao.focus();
        }
    });
});

// Chips de filtro (Compras, Produtos, Pedidos, Clientes...): o leitor de tela diz qual está escolhido.
// Cada tela troca a classe "ativo"; aqui o aria-pressed acompanha a classe.
document.addEventListener('DOMContentLoaded', () => {
    const chips = document.querySelectorAll('button.chip');
    const sincronizar = chip => chip.setAttribute('aria-pressed', chip.classList.contains('ativo'));
    chips.forEach(chip => {
        sincronizar(chip);
        new MutationObserver(() => sincronizar(chip)).observe(chip, { attributes: true, attributeFilter: ['class'] });
    });
});

// ---------- Peças das telas do painel (Produtos, Pedidos, Clientes, Calendário) ----------

// Janela (.modal-fundo): abre com .abrir(); fecha pelos botões indicados, clicando fora ou com Esc
function janela(id, ...botoesFechar) {
    const fundo = document.getElementById(id);
    const fechar = () => fundo.classList.remove('aberto');

    botoesFechar.forEach(b => document.getElementById(b).addEventListener('click', fechar));
    fundo.addEventListener('click', e => {
        if (e.target === fundo) fechar();
    });
    document.addEventListener('keydown', e => {
        if (e.key === 'Escape' && fundo.classList.contains('aberto')) fechar();
    });

    return { abrir: () => fundo.classList.add('aberto'), fechar };
}

// Chips de filtro: marca o escolhido e avisa com o valor do atributo data-<campo> (ex.: data-status)
function chipsDeFiltro(campo, aoEscolher) {
    const chips = document.querySelectorAll('.chip');
    chips.forEach(chip => chip.addEventListener('click', () => {
        chips.forEach(c => c.classList.remove('ativo'));
        chip.classList.add('ativo');
        aoEscolher(chip.dataset[campo]);
    }));
}

// Tabela que mostra a lista aos poucos: as primeiras linhas e um botão "Mostrar mais".
// A busca e os filtros continuam valendo para a lista inteira; só o desenho é dividido.
function tabelaPaginada({ corpo, contador, vazio, linha, singular, plural, porPagina = 50 }) {
    let lista = [];
    let limite = porPagina;

    const mais = document.createElement('button');
    mais.type = 'button';
    mais.className = 'btn-secundario btn-mais';
    (corpo.closest('.tabela-wrap') || corpo).insertAdjacentElement('afterend', mais);
    mais.addEventListener('click', () => {
        limite += porPagina;
        desenhar();
    });

    function desenhar() {
        const visiveis = lista.slice(0, limite);
        corpo.innerHTML = visiveis.map(linha).join('');

        const total = lista.length + ' ' + (lista.length === 1 ? singular : plural);
        contador.textContent = visiveis.length < lista.length ? `${total} • mostrando ${visiveis.length}` : total;
        vazio.style.display = lista.length === 0 ? 'block' : 'none';

        const restantes = lista.length - visiveis.length;
        mais.hidden = restantes <= 0;
        mais.textContent = `Mostrar mais (${restantes} ${restantes === 1 ? 'restante' : 'restantes'})`;
    }

    return {
        // Busca ou filtro novo volta para o começo; depois de salvar algo, a tela fica onde estava
        mostrar(novaLista, { manterPosicao = false } = {}) {
            lista = novaLista;
            if (!manterPosicao) limite = porPagina;
            desenhar();
        },
    };
}
