using BibliotecaApi.Application.Api.Auth;
using BibliotecaApi.Application.Api.Responses;
using BibliotecaApi.Domain.Entities;
using BibliotecaApi.UseCases.Livro;
using BibliotecaApi.UseCases.Livro.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BibliotecaApi.Application.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class LivroController : Controller
{
    private readonly CadastrarLivroUC _cadastrarLivroUC = new CadastrarLivroUC();
    private readonly ListarLivroUC _listarLivrosUC = new ListarLivroUC();

 
    [HttpPost]
    [Authorize(AuthenticationSchemes = ApiTokenAuthenticationDefaults.Scheme)]
    [SwaggerOperation(Summary = "Adiciona uma nova categoria retornando o seu respectivo Id")]
    [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(ApiResponse<int>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cadastrar(CadastrarLivroInputDTO input)
    {
        try
        {
            int newId = await _cadastrarLivroUC.Execute(input);
            return Ok(ApiResponse<int>.Ok(newId));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<int>.Falha("Erro ao cadastrar livro: " + ex.Message));
        }

    }

    [HttpGet]
    [SwaggerOperation(Summary = "Retorna a lista de livros cadastrados")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<LivroEntity>>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Listar()
    {
        try
        {
            List<LivroEntity>  listaLivros = await _listarLivrosUC.Execute();
            return Ok(ApiResponse<List<LivroEntity>>.Ok(listaLivros));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<LivroEntity>>.Falha("Erro ao listar livros: " + ex.Message));
        }

    }
}
