# Recursos Automáticos do CelmiBluetooth.Maui

## Como Funciona

A partir da versão 1.1.0, o módulo CelmiBluetooth.Maui inclui automaticamente todos os seus recursos (Colors.xaml e Styles.xaml) quando você chama `.AddCelmiBluetoothServices()` no seu MauiProgram.cs.

## Configuração no Projeto Principal

### 1. MauiProgram.cs
```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        // ... outras configurações ...
        .AddCelmiBluetoothServices() // ✅ Isso inclui automaticamente todos os recursos!
        .RegisterServices()
        .RegisterViewModels()
        .RegisterViews();

    return builder.Build();
}
```

### 2. App.xaml
```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- ✅ Recursos incluídos automaticamente através da extensão -->
            <!-- Colors.xaml e Styles.xaml vêm do módulo automaticamente -->
        </ResourceDictionary.MergedDictionaries>
        
        <!-- Seus converters e recursos específicos do projeto -->
        <converters:MeuConverter x:Key="MeuConverter" />
    </ResourceDictionary>
</Application.Resources>
```

## Recursos Disponíveis Automaticamente

### Colors.xaml
- Todas as cores do sistema Celmi (CorPrincipal, CorFundo, etc.)
- Cores padrão do .NET MAUI (Primary, Secondary, etc.)
- Brushes correspondentes

### Styles.xaml  
- Estilos para todos os controles padrão
- Estilos específicos do Syncfusion
- Estilos personalizados Celmi

## Vantagens

✅ **Sem duplicação**: Não precisa copiar arquivos de recursos  
✅ **Atualizações automáticas**: Novos estilos chegam com as atualizações do módulo  
✅ **Consistência**: Todos os projetos usam os mesmos recursos  
✅ **Simplicidade**: Apenas uma linha no MauiProgram.cs  

## Migração de Projetos Existentes

Se seu projeto já tinha cópias locais dos recursos:

1. **Remova** as pastas `Resources/Styles/Colors.xaml` e `Resources/Styles/Styles.xaml`
2. **Remova** as referências no `csproj`:
   ```xml
   <!-- ❌ Remover estas linhas -->
   <MauiXaml Update="Resources\Styles\Colors.xaml">
     <Generator>MSBuild:Compile</Generator>
   </MauiXaml>
   ```
3. **Remova** as referências no `App.xaml`:
   ```xml
   <!-- ❌ Remover estas linhas -->
   <ResourceDictionary Source="/Resources/Styles/Colors.xaml" />
   <ResourceDictionary Source="/Resources/Styles/Styles.xaml" />
   ```
4. **Certifique-se** de que está chamando `.AddCelmiBluetoothServices()` no MauiProgram.cs

## Customização

Se precisar sobrescrever algum estilo ou cor:

```xml
<Application.Resources>
    <ResourceDictionary>
        <!-- Recursos do módulo incluídos automaticamente -->
        
        <!-- Suas customizações -->
        <Color x:Key="Primary">#FF0000</Color> <!-- Sobrescreve a cor primária -->
        
        <Style x:Key="MeuButtonStyle" TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">
            <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
        </Style>
    </ResourceDictionary>
</Application.Resources>
```

## Verificação

Para verificar se os recursos estão carregados corretamente, você pode injetar `ICelmiResourceProvider`:

```csharp
public class MinhaViewModel
{
    public MinhaViewModel(ICelmiResourceProvider resourceProvider)
    {
        if (resourceProvider.ResourcesAvailable)
        {
            // Recursos carregados com sucesso!
        }
    }
}
```