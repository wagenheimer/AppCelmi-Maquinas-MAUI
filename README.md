# AppCelmi - Máquinas - MAUI

AppCelmi - Pecuaria é um aplicativo multiplataforma desenvolvido com .NET MAUI, voltado para o gerenciamento de máquinas e processos no setor pecuário.

## Tecnologias e Bibliotecas
- **.NET 9**
- **.NET MAUI** (Android, iOS, Windows, MacCatalyst)
- **MVVM** com [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- [Shiny](https://shinyorg.github.io/) (serviços de background)
- [Syncfusion](https://www.syncfusion.com/maui-controls) (controles avançados)
- [FontAwesome](https://fontawesome.com/)
- [LocalizationResourceManager.Maui](https://github.com/CrossGeeks/LocalizationResourceManager.Maui)

## Estrutura do Projeto
- `Views/` - Telas e páginas do aplicativo
- `ViewModels/` - Lógica de apresentação (MVVM)
- `Resources/` - Estilos, cores, fontes e recursos de localização
- `Utils/` - Utilitários e helpers

## Padrões e Boas Práticas
- Uso extensivo de `[ObservableProperty]` e `[RelayCommand]` do CommunityToolkit.Mvvm
- Injeção de dependência e uso de `async/await` para operações assíncronas
- Registro de fontes, handlers e recursos em `MauiProgram.cs`
- Organização e limpeza do XAML
- Tratamento de exceções e mensagens amigáveis ao usuário

> Este projeto segue as melhores práticas de .NET MAUI e MVVM. Para mais detalhes, consulte o arquivo `.github/copilot-instructions.md`.
