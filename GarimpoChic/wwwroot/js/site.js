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

    return fetch(url, opcoes).then(r => {
        if (!r.ok) throw new Error('Erro ' + r.status);
        return (r.headers.get('content-type') || '').includes('json') ? r.json() : null;
    });
}

function falhou() {
    alert('Não foi possível salvar. Verifique sua conexão e tente novamente.');
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
