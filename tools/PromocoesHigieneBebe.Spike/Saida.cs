using System.Globalization;
using System.Text;

namespace PromocoesHigieneBebe.Spike;

/// Grava as respostas como fixtures e monta o relatório da sondagem.
internal sealed class Saida
{
    private readonly StringBuilder _relatorio = new();

    private Saida(string raizDoRepositorio)
    {
        PastaDeFixtures = Path.Combine(raizDoRepositorio, "tests", "PromocoesHigieneBebe.Infrastructure.Tests", "Fixtures");
        CaminhoDoRelatorio = Path.Combine(raizDoRepositorio, "tools", "PromocoesHigieneBebe.Spike", "RELATORIO.md");
        _relatorio.AppendLine("# Relatório da Fase 0")
                  .AppendLine()
                  .AppendLine(CultureInfo.InvariantCulture, $"Gerado em {TimeProvider.System.GetLocalNow():yyyy-MM-dd HH:mm zzz}.")
                  .AppendLine();
    }

    public string PastaDeFixtures { get; }

    public string CaminhoDoRelatorio { get; }

    public static Saida Criar() => new(EncontrarRaizDoRepositorio());

    public string GravarFixture(string drogaria, string nomeDoArquivo, string conteudo)
    {
        var pasta = Path.Combine(PastaDeFixtures, drogaria);
        Directory.CreateDirectory(pasta);
        var caminho = Path.Combine(pasta, nomeDoArquivo);
        File.WriteAllText(caminho, conteudo, Encoding.UTF8);
        return caminho;
    }

    public void Titulo(string texto)
    {
        _relatorio.AppendLine().AppendLine(CultureInfo.InvariantCulture, $"## {texto}").AppendLine();
        Console.WriteLine($"\n== {texto}");
    }

    public void Linha(string texto)
    {
        _relatorio.AppendLine(CultureInfo.InvariantCulture, $"- {texto}");
        Console.WriteLine($"  {texto}");
    }

    public void GravarRelatorio() => File.WriteAllText(CaminhoDoRelatorio, _relatorio.ToString(), Encoding.UTF8);

    private static string EncontrarRaizDoRepositorio()
    {
        var pasta = new DirectoryInfo(AppContext.BaseDirectory);
        while (pasta is not null && !Directory.Exists(Path.Combine(pasta.FullName, ".git")))
        {
            pasta = pasta.Parent;
        }

        return pasta?.FullName ?? Directory.GetCurrentDirectory();
    }
}
