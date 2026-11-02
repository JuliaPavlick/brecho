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
        headers: { 'RequestVerificationToken': document.querySelector('meta[name="csrf"]').content }
    };

    if (corpo instanceof FormData) {
        opcoes.body = corpo;
    } else if (corpo !== undefined) {
        opcoes.headers['Content-Type'] = 'application/json';
        opcoes.body = JSON.stringify(corpo);
    }

    Carregando.inicio();
    return fetch(url, opcoes).then(async r => {
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

function mostrarErro(id, ok) {
    document.getElementById(id).style.display = ok ? 'none' : 'block';
    return ok;
}

// Botão "Adicionar à sacola" dos cards (vale para todas as páginas da área do cliente)
document.addEventListener('click', e => {
    const btn = e.target.closest('.btn-sacola');
    if (!btn) return;

    btn.disabled = true;
    postar('/sacola/' + btn.dataset.id)
        .then(r => {
            btn.classList.toggle('ativo', r.naSacola);
            btn.textContent = r.naSacola ? 'Na sacola ✓' : 'Adicionar à sacola';

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
    } finally {
        botoes.forEach(b => b.disabled = false);
    }
});

// Formulário enviado do jeito normal (a página vai recarregar): mostra "Carregando…" e ignora novos envios.
// Fica por último: as validações e o data-confirmar acima já rodaram e podem ter cancelado o envio.
document.addEventListener('submit', e => {
    const form = e.target;
    if (e.defaultPrevented) return;

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
