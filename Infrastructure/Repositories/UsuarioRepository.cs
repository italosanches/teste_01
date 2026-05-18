using Dapper;
using BibliotecaApi.Domain.Entities;
using BibliotecaApi.Infrastructure.Data;

namespace BibliotecaApi.Infrastructure.Repositories;

public class UsuarioRepository
{
    private readonly DbSession _session;

    public UsuarioRepository()
    {
        _session = new DbSession(ConfigurationHelper.GetConfiguration());
    }

    public async Task<int> Cadastrar(UsuarioEntity usuario)
    {
        const string sql = "INSERT INTO Usuarios (nome, cpf, email, tem_livro_em_atraso) VALUES (@nome, @cpf, @email, @temLivroEmAtraso) RETURNING id";

        var parameters = new
        {
            nome = usuario.Nome,
            cpf = usuario.CPF,
            email = usuario.Email,
            temLivroEmAtraso = false
        };

        return await _session.Connection.QueryFirstAsync<int>(sql, parameters);
    }

    public async Task AtualizarInadimplencia(int idUsuario, bool temLivroEmAtraso)
    {
        const string sql = "UPDATE Usuarios SET tem_livro_em_atraso = @temLivroEmAtraso WHERE id = @idUsuario";
        await _session.Connection.ExecuteAsync(sql, new { idUsuario, temLivroEmAtraso });
    }

    public async Task<bool> VerificaCpfNoDb(string cpf)
    {
        const string sql = "SELECT COUNT(1) FROM Usuarios WHERE cpf = @cpf";
        int count = await _session.Connection.QueryFirstAsync<int>(sql, new { cpf });
        return count > 0;
    }
}
