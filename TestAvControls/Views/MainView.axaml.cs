using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using AJBAvalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TestAvControls.Views;

public partial class MainView : UserControl
{
    public static readonly DirectProperty<MainView, ObservableCollection<string>> MyDataProperty = AvaloniaProperty.RegisterDirect<MainView, ObservableCollection<string>>(nameof(MyData), o => o.MyData);

    private ObservableCollection<string> _myData = new ObservableCollection<string>() {"one", "two", "three", "four"};
    
    public ObservableCollection<string> MyData 
    {
        get => _myData;
        private set => SetAndRaise(MyDataProperty, ref _myData, value);
    }
    public MainView()
    {
       
        InitializeComponent();
        treeView.ItemsSource = new List<string>() {"one", "two", "three", "four"};
        treeView.SelectionChanged += TreeViewOnSelectionChanged;
        dropControl.OnClearEvent += DropControlOnOnClearEvent;
        //cb.ItemsSource = new List<string>() {"one", "two", "three", "four"};
        var cbList = new ObservableCollection<Person>();
        cbList.Add(new Person("fred",45));
        cbList.Add(new Person("mike",30));
        cbList.Add(new Person("jane",26));
        comboBoxEx.ItemsSource = cbList;
        checkComboBoxEx.ItemsSource = cbList;
        listSelect.LoadLists(cbList, null, "Name");
    }

    private void DropControlOnOnClearEvent(object? sender, EventArgs e)
    {
        treeView.SelectedItem = null;
    }

    private void TreeViewOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        dropControl.EditorValue = treeView.SelectedItem?.ToString();
        dropControl.CloseDropDown();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        //hideMe.IsHidden = !hideMe.IsHidden;
    }
}

public class Person
{
    public string Name { get; }
    public int Age { get; }

    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
}
