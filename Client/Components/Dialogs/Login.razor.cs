using GrpcAuth.Contracts.Models.Requests;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GrpcAuth.Client.Components.Dialogs;

public partial class Login
{
    private readonly TokenRequest _tokenModel = new();
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    private bool _passwordVisibility;
    
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = default!;

    private async Task SubmitAsync()
    {
        var result = await AuthenticationManager.Login(_tokenModel);
        if (result.Succeeded)
        {
            Snackbar.Add($"Welcome {_tokenModel.Email}", Severity.Success);
            NavigationManager.NavigateTo("/", true);
            MudDialog.Close(DialogResult.Ok(true));
        }
        else
            foreach (var message in result.Messages)
                Snackbar.Add(message, Severity.Error);
    }
    private async Task ShowRegisterDialog()
    {
        var parameters = new DialogParameters();
        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            DisableBackdropClick = true
        };
        var dialog = DialogService.Show<Register>(
            "Register",
            parameters,
            options);
        await dialog.Result;
    }

    private void Cancel() =>  MudDialog.Cancel();

    private void TogglePasswordVisibility()
    {
        if (_passwordVisibility)
        {
            _passwordVisibility = false;
            _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
            _passwordInput = InputType.Password;
            
            return;
        }
        
        _passwordVisibility = true;
        _passwordInputIcon = Icons.Material.Filled.Visibility;
        _passwordInput = InputType.Text;
    }

    private void FillAdministratorCredentials()
    {
        _tokenModel.Email = "john@email.co.uk";
        _tokenModel.Password = "123Pa$$word!";
    }

    private void FillBasicUserCredentials()
    {
        _tokenModel.Email = "jane@email.co.uk";
        _tokenModel.Password = "123Pa$$word!";
    }
}