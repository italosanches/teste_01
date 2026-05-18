
using Dapper;
using BibliotecaApi.Infrastructure.Data;
using BibliotecaApi.Domain.Entities;

namespace BibliotecaApi.Infrastructure.Repositories;


public class LivroRepository
{
    private readonly DbSession _session;
    public LivroRepository()
    {
        _session = new DbSession(ConfigurationHelper.GetConfiguration());
    }

    public async Task<int> Cadastrar(LivroEntity livro)
    {
        const string sql = "INSERT INTO Livros(titulo, autor, isbn) VALUES(@titulo, @autor, @isbn) RETURNING id";

        var parameters = new { titulo = livro.Titulo, autor = livro.Autor, isbn = livro.ISBN };

        return await _session.Connection.QueryFirstAsync<int>(sql, parameters);
    }

    public async Task MarcarComoIndisponivel(int idLivro)
    {
        const string sql = "UPDATE Livros SET disponivel = FALSE WHERE id = @id";
        await _session.Connection.ExecuteAsync(sql, new { id = idLivro });
    }

    public async Task MarcarComoDisponivel(int idLivro)
    {
        const string sql = "UPDATE Livros SET disponivel = TRUE WHERE id = @id";
        await _session.Connection.ExecuteAsync(sql, new { id = idLivro });
    }

    public async Task<List<LivroEntity>> ListarLivros()
    {
        const string sql = "SELECT * FROM Livros";
        var livros = await _session.Connection.QueryAsync<LivroEntity>(sql);
        return livros.ToList();
    }

}
