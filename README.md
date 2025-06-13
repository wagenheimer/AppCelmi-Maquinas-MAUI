# AppCelmi - Pecuaria

AppCelmi - Pecuaria � um aplicativo multiplataforma desenvolvido com .NET MAUI, voltado para o gerenciamento de m�quinas e processos no setor pecu�rio.

## Tecnologias e Bibliotecas
- **.NET 9**
- **.NET MAUI** (Android, iOS, Windows, MacCatalyst)
- **MVVM** com [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- [Shiny](https://shinyorg.github.io/) (servi�os de background)
- [Syncfusion](https://www.syncfusion.com/maui-controls) (controles avan�ados)
- [FontAwesome](https://fontawesome.com/)
- [LocalizationResourceManager.Maui](https://github.com/CrossGeeks/LocalizationResourceManager.Maui)

## Estrutura do Projeto
- `Views/` - Telas e p�ginas do aplicativo
- `ViewModels/` - L�gica de apresenta��o (MVVM)
- `Resources/` - Estilos, cores, fontes e recursos de localiza��o
- `Utils/` - Utilit�rios e helpers

## Padr�es e Boas Pr�ticas
- Uso extensivo de `[ObservableProperty]` e `[RelayCommand]` do CommunityToolkit.Mvvm
- Inje��o de depend�ncia e uso de `async/await` para opera��es ass�ncronas
- Registro de fontes, handlers e recursos em `MauiProgram.cs`
- Organiza��o e limpeza do XAML
- Tratamento de exce��es e mensagens amig�veis ao usu�rio

> Este projeto segue as melhores pr�ticas de .NET MAUI e MVVM. Para mais detalhes, consulte o arquivo `.github/copilot-instructions.md`.