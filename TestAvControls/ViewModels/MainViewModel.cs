using Avalonia;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using TestAvControls.Views;

namespace TestAvControls.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";

    private int _numberValue = 0;

    private ObservableCollection<string> _myData = new ObservableCollection<string>() { "one", "two", "three", "four" };
    public ObservableCollection<string> MyData
    {
        get => _myData;
        private set => this.RaiseAndSetIfChanged(ref _myData, value);
    }

    public int NumberValue
    {
        get => _numberValue;
        set => this.RaiseAndSetIfChanged(ref _numberValue, value);
    }
}
