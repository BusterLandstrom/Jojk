using Avalonia.Controls;
using Avalonia.Input;

namespace Jojk.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
		private void TopBar_PointerPressed(object sender, PointerPressedEventArgs e)
		{
			if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
				BeginMoveDrag(e);
		}
	}
}