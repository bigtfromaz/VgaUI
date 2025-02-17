using Microsoft.AspNetCore.Components;

namespace VgaUI.Client;

public class ActiveTabService
{
    public string ActiveTab { get; private set; } = "Home";

    public event Action? OnChange;


    public void ChangeActiveTab(string tabName)
    {
        ActiveTab = tabName;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}