##################### Desafio Umbler #####################

Este repositório contém a minha solução para o desafio técnico da Umbler. O foco principal desta entrega não foi apenas fazer o código funcionar, mas aplicar boas práticas como SOLID e Clean Code para transformar uma rota altamente acoplada em uma API testável, segura e de fácil manutenção.

# O que foi refatorado?

Ao analisar o código do `DomainController`, identifiquei as possíveis melhorias e atuei nas seguintes frentes:

1. Desacoplamento e Injeção de Dependência (DI)
   - Problema: O Controller instanciava bibliotecas externas (`LookupClient` e `WhoisClient`) diretamente no método, impossibilitando testes unitários limpos.
   - Solução: Criei as interfaces `IDnsService` e `IWhoisService` e isolei a comunicação externa. Agora o Controller depende de abstrações injetadas via construtor.

2. Segurança e Otimização de Payload (DTOs)
   - Problema: A API retornava a entidade `Domain` inteira do banco de dados (vazando propriedades internas como `Id`).
   - Solução: Implementação do padrão DTO (`DomainResponseDTO`) para blindar o modelo de domínio e trafegar apenas os dados necessários para o Front.

3. Redução de Complexidade Ciclomática
   - Problema: A lógica de consulta ao Whois/DNS estava duplicada (uma no cenário de domínio inexistente e outra na expiração do TTL).
   - Solução: Unificação dos blocos lógicos (if (domain == null || expirou)), reduzindo o tamanho do método pela metade e facilitando a leitura.

4. Testes Unitários com Mocks
   - Problema: Os testes dependiam de conexão com a internet para passar e estavam comentados.
   - Solução: Refatoração da suíte de testes usando a biblioteca *Moq*. Agora os serviços de DNS e Whois são "mockados", garantindo testes rápidos, determinísticos e isolados de falhas de rede.

5. Flexibilidade de Infraestrutura (Entity Framework Core)
   - Devido a bloqueios de firewall locais (Porta 3306) com o banco MySQL da hospedagem inicial, tomei a decisão de alterar o provedor do EF Core para *SQL Server* no ambiente de desenvolvimento local.
   - Isso demonstra o poder do ORM: a mudança custou apenas um pacote NuGet e a alteração do `UseSqlServer`, sem impactar absolutamente nenhuma regra de negócio.

#️ Como executar o projeto localmente?

1. Clone o repositório.
2. Certifique-se de ter o *.NET 6 SDK* e o *Node.js* instalados.
3. No arquivo `appsettings.json`, ajuste a `DefaultConnection` para a string de conexão do meu SQL Server (ou LocalDB).
4. Abra o terminal na pasta raiz do projeto (`src/Desafio.Umbler`) e execute os comandos no powershell:
   dotnet ef database update
   npm install
   npm run build
   dotnet run
