# Copilot Instructions

## Context
- This repository targets .NET 9 and uses .NET MAUI with the MVVM pattern (CommunityToolkit.Mvvm).
- Do not suggest Xamarin.Forms solutions; always use .NET MAUI equivalents.
- Main libraries: CommunityToolkit.Mvvm, Shiny, Syncfusion, FontAwesome, LocalizationResourceManager.Maui.

## Coding Guidelines
- Use C# and .NET MAUI best practices.
- Always use `[ObservableProperty]` and `[RelayCommand]` from CommunityToolkit.Mvvm to reduce boilerplate in ViewModels.
- Place ViewModels in the `ViewModel` folder and Views in the `Views` folder.
- Register custom/third-party handlers in `MauiProgram.cs` as needed (e.g., Syncfusion controls).
- Use dependency injection and `async/await` for asynchronous operations.
- Register fonts and resources (e.g., FontAwesome, localization) in `MauiProgram.cs`.
- Reference colors and styles from `Resources/Styles/Colors.xaml` and `Resources/Styles/Styles.xaml` using `{StaticResource}`.
- Prefer using `var` instead of explicit variable types for local variable declarations, unless clarity is lost.
- Whenever possible, invert `if` statements to avoid nesting and improve code readability.
- Use XML documentation comments (`///`) for all public classes, methods, and properties.
- Organize class members in the following order: private fields, properties, constructors, public methods, private methods, partial methods.
- Use partial methods (e.g., `partial void OnPropertyChanged`) for handling logic related to `[ObservableProperty]` changes.
- Prefer `ObservableCollection<T>` for collections bound to the UI.
- Always handle exceptions in async methods that interact with external resources, and log or display user-friendly messages.
- When serializing or deserializing data, always validate the result and handle possible conversion errors gracefully.
- Use PascalCase for classes, methods, and properties; camelCase for private fields; prefix backing fields with `_`.
- If a property depends on localization, always override `UpdateLocalizedProperties` in the relevant ViewModel.

## XAML and Project Structure
- Keep XAML clean: remove unused namespaces and keep markup readable.
- Use correct namespaces for ViewModels, Views, FontAwesome, and Syncfusion controls.
- Maintain organized folders: `Views`, `ViewModels`, `Utils`, `Resources`, etc.

## Additional Notes
- Avoid deprecated APIs.
- Write clear and descriptive commit messages and pull request descriptions.
- Ensure all code builds and passes unit tests before submitting changes.
