using System.Windows.Controls;
using CryptoTools.ViewModels;

namespace CryptoTools.Views;

public partial class HashPage
{
    private readonly HashPageViewModel _viewModel;
    public HashPage()
    {
        InitializeComponent();
        
        _viewModel = (HashPageViewModel)DataContext;
    }

    private void TextToHash_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.UnhashedText = ((TextBox)sender).Text;
        _viewModel.HashText();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _viewModel?.HashText();
    }
}