using System.Windows;
using System.Windows.Input;
using shape_builder.ViewModel;

namespace shape_builder;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(DrawingCanvas);
    }
    
    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            Point position = e.GetPosition(DrawingCanvas);
            
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null && viewModel.SelectShapeCommand.CanExecute(position))
            {
                viewModel.SelectShapeCommand.Execute(position);   
                DrawingCanvas.CaptureMouse();
            }
        }
    }
    
    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            Point position = e.GetPosition(DrawingCanvas);
            
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null && viewModel.MoveShapeCommand.CanExecute(position))
            {
                viewModel.MoveShapeCommand.Execute(position);    
            }
        }
    }

    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (DrawingCanvas.IsMouseCaptured)
        {
            DrawingCanvas.ReleaseMouseCapture();
            Point position = e.GetPosition(DrawingCanvas);
            
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null && viewModel.MoveShapeCommand.CanExecute(position))
            {
                viewModel.MoveShapeCommand.Execute(null);    
            }
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null && viewModel.RemoveShapeCommand.CanExecute(true))
            {
                viewModel.RemoveShapeCommand.Execute(true);
            }
        }
    }
}
