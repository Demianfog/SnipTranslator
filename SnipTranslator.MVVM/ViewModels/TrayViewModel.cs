using System.Reactive;
using ReactiveUI;

namespace SnipTranslator.MVVM.ViewModels;

public class TrayViewModel : ViewModelBase
{
    public TrayViewModel()
    {
        ExitCommand = ReactiveCommand.Create(ExitCommandFunc);
        
        ToggleCommand = ReactiveCommand.Create(ToggleCommandFunc);
    }

    private void ToggleCommandFunc()
    {
        throw new System.NotImplementedException();
    }

    private void ExitCommandFunc()
    {
        System.Environment.Exit(0);
    }

    public ReactiveCommand<Unit, Unit> ExitCommand { get; }
    
    public ReactiveCommand<Unit,Unit> ToggleCommand { get; }
}