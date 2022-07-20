using GrpcAuth.Contracts.Models.Requests;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GrpcAuth.Client.Components.Dialogs;

public partial class Register
{
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

    private bool _passwordVisibility;
    private RegisterRequest _registerUserModel = new();
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = default!;

    private async Task SubmitAsync()
    {
        var response = await UserService.RegisterAsync(_registerUserModel);
        if (response.Succeeded)
        {
            Snackbar.Add(response.Messages[0], Severity.Success);
            _registerUserModel = new RegisterRequest();
            MudDialog.Close(DialogResult.Ok(true));
        }
        else
            foreach (var message in response.Messages)
                Snackbar.Add(message, Severity.Error);
    }
    
    private void Cancel() =>  MudDialog.Cancel();

    private void TogglePasswordVisibility()
    {
        if (_passwordVisibility)
        {
            _passwordVisibility = false;
            _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
            _passwordInput = InputType.Password;
        }
        else
        {
            _passwordVisibility = true;
            _passwordInputIcon = Icons.Material.Filled.Visibility;
            _passwordInput = InputType.Text;
        }
    }
}