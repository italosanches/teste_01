# Biblioteca API

API de gerenciamento de biblioteca desenvolvida em **.NET 8**, com cadastro de usuários, livros e empréstimos.

---

### 1. FEATURE: Validar CPF do usuário — ✅ Implementado

**Requisito:** No cadastro de usuário, validar se o CPF já existe antes de cadastrar. Caso o CPF já esteja cadastrado, retornar erro.

**Implementação:**
- Use case `CadastrarUsuarioUC` consulta o banco via `UsuarioRepository.VerificaCpfNoDb` antes de persistir.
- Mensagem retornada: `"Usuário com este CPF já está cadastrado."`
---

### 2. FEATURE: Validar ISBN do livro — ✅ Implementado

**Requisito:** O campo ISBN deve conter exatamente 13 dígitos numéricos. Caso o formato esteja incorreto, retornar erro apropriado.

**Implementação:**
- Validação na entidade `LivroEntity.ValidarDados`:
  - ISBN obrigatório
  - Apenas números
  - Exatamente 13 dígitos

**Mensagens de erro:** `"ISBN não pode ser vazio."`, `"ISBN deve conter apenas números."`, `"ISBN deve conter exatamente 13 dígitos."`

---

### 3. BUG: Gerando multa mesmo quando o empréstimo é devolvido no prazo — ✅ Implementado

**Requisito:** A multa deve ser gerada somente se a devolução ocorrer após a `data_prevista_devolucao`.

**Implementação:**
- Método `EmprestimoEntity.CalcularMulta()` compara as datas por dia civil (`.Date`).
- Se a devolução for no prazo ou antes: `Multa = 0`.

---

### 4. BUG: Permite emprestar um livro que já está emprestado — ✅ Implementado

**Requisito:** Antes de registrar um novo empréstimo, verificar se o livro já está emprestado e ainda não foi devolvido. Caso esteja, retornar:

```json
{
  "sucesso": false,
  "conteudo": null,
  "mensagem_erro": "Este livro já está emprestado e ainda não foi devolvido."
}
```

**Implementação:**
- `CadastrarEmprestimoUC` chama `EmprestimoRepository.ExisteEmprestimoAtivoPorLivro` antes do `INSERT`.
- Consulta empréstimos com `data_devolucao IS NULL` para o `id_livro` informado.
- Após emprestar com sucesso, o livro é marcado como indisponível (`MarcarComoIndisponivel`).
---

### 5. FEATURE: Listar todos os livros — ✅ Implementado

**Requisito:** Criar endpoint `GET /livro/listar` que retorne a lista completa de livros cadastrados.

**Implementação:**
- Endpoint `GET /Livro/Listar` em `LivroController` (roteamento case-insensitive → `/livro/listar`).
- Use case `ListarLivroUC` delega ao `LivroRepository.ListarLivros`.
- Consulta `SELECT * FROM Livros` — retorna **todos** os livros (emprestados ou disponíveis).

**Exemplo de resposta:**

```json
{
  "sucesso": true,
  "conteudo": [
    {
      "id": 1,
      "titulo": "O Senhor dos Anéis",
      "autor": "J.R.R. Tolkien",
      "isbn": "9788533613370"
    }
  ],
  "mensagem_erro": null
}
```

---

### 6. FEATURE: Impedir novo empréstimo se o usuário tiver atraso — ✅ Implementado

**Requisito:** Validar se o usuário possui algum empréstimo em atraso (ainda não devolvido e `data_prevista_devolucao < DateTime.Now`). Caso possua, impedir novo empréstimo e retornar:

```json
{
  "sucesso": false,
  "conteudo": null,
  "mensagem_erro": "Usuário com empréstimo em atraso não pode realizar novo empréstimo."
}
```

Criar marcação no banco de dados para indicar se o usuário possui atraso ativo.

**Implementação:**
- `CadastrarEmprestimoUC` chama `EmprestimoRepository.ExisteEmprestimoEmAtrasoPorUsuario` antes de cadastrar o empréstimo.
- Consulta empréstimos com `data_devolucao IS NULL` e `data_prevista_devolucao < DateTime.Now`.
- Ao bloquear, atualiza `tem_livro_em_atraso = true` via `UsuarioRepository.AtualizarInadimplencia`.
- Na devolução (`DevolverEmprestimoUC`), recalcula se o usuário ainda tem outro empréstimo em atraso e atualiza a flag (`true` ou `false`).
- Propriedade na entidade: `UsuarioEntity.EmprestimoEmAtraso` (espelha a coluna do banco).

**Coluna no banco (SQLite):**

```sql
ALTER TABLE Usuarios
ADD COLUMN tem_livro_em_atraso INTEGER NOT NULL DEFAULT 0;
```

---

### 7. FEATURE: Ajustar regra de multa — ✅ Implementado

**Requisito:** Nova lógica de cálculo em `CalcularMulta()`:

- Até 3 dias de atraso → R$ 2,00 por dia
- A partir do 4º dia → R$ 3,50 por dia
- Limite máximo → R$ 50,00
---

### 8. FEATURE: Autenticação (API Token / Bearer) — ✅ Implementado

**Requisito:** Proteger endpoints sensíveis com API Token. Retornar **401 Unauthorized** quando o token for inválido ou ausente.

**Endpoints protegidos:**
- `POST /Livro/Cadastrar`
- `POST /Emprestimo/Cadastrar`
- `POST /Emprestimo/Devolver`

**Endpoints públicos (sem token):**
- `POST /Usuario/Cadastrar`
- `GET /Livro/Listar`

---

## Adendo — correção extra na devolução

Durante os testes, foi identificado um **bug adicional** no fluxo de devolução:

- **Problema:** Em `DevolverEmprestimoUC`, após `Atualizar` o empréstimo existente, era chamado `Cadastrar`, o que criava um **novo registro** de empréstimo no banco (comportamento incorreto).
- **Correção:** Removida a chamada `Cadastrar` na devolução. O fluxo correto passou a ser apenas:
  1. `RegistrarDevolucao()` na entidade
  2. `Atualizar(emprestimo)` no repositório
  3. `MarcarComoDisponivel(idLivro)` no livro

**Arquivo:** `UseCases/Emprestimo/DevolverEmprestimoUC.cs`

---
