// Máscara de CPF
document.getElementById('cpf').addEventListener('input', function () {
  let v = this.value.replace(/\D/g, '');
  if (v.length > 3)  v = v.slice(0, 3) + '.' + v.slice(3);
  if (v.length > 7)  v = v.slice(0, 7) + '.' + v.slice(7);
  if (v.length > 11) v = v.slice(0, 11) + '-' + v.slice(11, 13);
  this.value = v;
});

// Validação antes do submit
document.querySelector('form').addEventListener('submit', function (e) {
  let valido = true;

  const cpf   = document.getElementById('cpf');
  const senha = document.getElementById('senha');
  const erroCpf   = document.getElementById('cpf-erro');
  const erroSenha = document.getElementById('senha-erro');

  // Limpa erros anteriores
  erroCpf.classList.remove('visivel');
  erroSenha.classList.remove('visivel');

  const cpfLimpo = cpf.value.replace(/\D/g, '');
  if (cpfLimpo.length !== 11) {
    erroCpf.textContent = 'Informe um CPF válido (11 dígitos).';
    erroCpf.classList.add('visivel');
    valido = false;
  }

  if (senha.value.trim() === '') {
    erroSenha.textContent = 'Preencha sua senha.';
    erroSenha.classList.add('visivel');
    valido = false;
  }

  if (!valido) e.preventDefault();
});
