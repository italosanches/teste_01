using BibliotecaApi.Domain.Entities;
using BibliotecaApi.Infrastructure.Repositories;

namespace BibliotecaApi.UseCases.Livro
{
    public class ListarLivroUC
    {
        private LivroRepository _repository = new LivroRepository();
        public async Task<List<LivroEntity>> Execute()
        {
            try
            {
                List<LivroEntity> livros = await _repository.ListarLivros();
                return livros;
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
