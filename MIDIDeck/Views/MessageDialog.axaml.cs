using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MIDIDeck.Views;

public partial class MessageDialog : Window
{
    private readonly TextBlock? _messageBlock;
    private readonly Button? _okButton;
    private readonly TextBlock? _titleBlock;

    public MessageDialog()
    {
        InitializeComponent();

        _titleBlock = this.FindControl<TextBlock>("TitleBlock");
        _messageBlock = this.FindControl<TextBlock>("MessageBlock");
        _okButton = this.FindControl<Button>("OkButton");

        if (_okButton != null)
            _okButton.Click += OkButton_Click;
    }

    public void SetText(string title, string message)
    {
        if (_titleBlock != null) _titleBlock.Text = title;
        if (_messageBlock != null) _messageBlock.Text = message;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}