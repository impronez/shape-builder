using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using shape_builder.Command;
using shape_builder.Model;
using Frame = shape_builder.Model.Frame;

namespace shape_builder.ViewModel;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<System.Windows.Shapes.Shape> Shapes { get; set; }

    private Point _startPoint;
    private bool _isShapeMoving;
    private System.Windows.Shapes.Shape? _selectedShape;
    private readonly double _canvasHeight;
    private readonly double _canvasWidth;
    private readonly Frame _selectedShapeFrame;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public MainViewModel(Canvas initCanvas)
    {
        Shapes = new ObservableCollection<System.Windows.Shapes.Shape>();
        _selectedShape = null;
        
        _canvasHeight = initCanvas.Height;
        _canvasWidth = initCanvas.Width;

        _selectedShapeFrame = new Frame(_canvasWidth, _canvasHeight);
    }
    
    private RelayCommand? _selectShapeCommand;
    public RelayCommand SelectShapeCommand => _selectShapeCommand ??= new RelayCommand(ExecuteSelectShapeCommand);

    private RelayCommand? _moveShapeCommand;
    public RelayCommand MoveShapeCommand => _moveShapeCommand ??= new RelayCommand(ExecuteMoveShapeCommand);

    private RelayCommand? _addShapeCommand;
    public RelayCommand AddShapeCommand => _addShapeCommand ??= new RelayCommand(ExecuteAddShapeCommand);

    private RelayCommand? _removeShapeCommand;
    public RelayCommand RemoveShapeCommand => _removeShapeCommand ??= new RelayCommand(_ => RemoveShape());
    
    private void ExecuteSelectShapeCommand(object? parameter)
    {
        if (parameter is Point cursorPosition)
        {
            var clickedShape = GetShapeAtMousePosition(cursorPosition);
            SelectShape(clickedShape, cursorPosition);
        }
    }

    private void ExecuteMoveShapeCommand(object? parameter)
    {
        if (parameter == null && _isShapeMoving)
        {
            _isShapeMoving = false;
        }
        else if (parameter is Point cursorPosition && _isShapeMoving && _selectedShape != null)
        {
            MoveShape(cursorPosition);
        }
    }

    private void ExecuteAddShapeCommand(object? parameter)
    {
        if (parameter is string shapeTypeString && Enum.TryParse(typeof(ShapeType), shapeTypeString, out var result))
        {
            AddShape((ShapeType)result);
        }
    }

    
    private void SelectShape(System.Windows.Shapes.Shape? shape, Point cursorPosition)
    {
        if (shape != null)
        {
            if (_selectedShape != null)
            {
                Panel.SetZIndex(_selectedShape, 0);
            }

            _selectedShape = shape;
            _selectedShapeFrame.SelectShape(_selectedShape);

            Panel.SetZIndex(_selectedShape, 1);
            _startPoint = cursorPosition;
            _isShapeMoving = true;

            if (!Shapes.Contains(_selectedShapeFrame))
            {
                Shapes.Add(_selectedShapeFrame);
            }

            Panel.SetZIndex(_selectedShapeFrame, 1);
        }
        else
        {
            DeselectShape();
        }
    }

    private void DeselectShape()
    {
        if (_selectedShape != null)
        {
            Panel.SetZIndex(_selectedShape, 0);
            _selectedShapeFrame.DeselectShape();
            _selectedShape = null;

            if (Shapes.Contains(_selectedShapeFrame))
            {
                Shapes.Remove(_selectedShapeFrame);
            }
        }
    }

    private double AdjustCoordinateWithinBounds(double coordinate, double size, double maxLimit, out bool isWithinBounds)
    {
        if (coordinate < 0)
        {
            isWithinBounds = false;
            return 0;
        }
        if (coordinate + size > maxLimit)
        {
            isWithinBounds = false;
            return maxLimit - size;
        }

        isWithinBounds = true;
        return coordinate;
    }
    
    private void MoveShape(Point cursorPosition)
    {
        if (_selectedShape == null) return;
        
        var offsetX = cursorPosition.X - _startPoint.X;
        var offsetY = cursorPosition.Y - _startPoint.Y;

        double left = Canvas.GetLeft(_selectedShape) + offsetX;
        double top = Canvas.GetTop(_selectedShape) + offsetY;

        left = AdjustCoordinateWithinBounds(left, _selectedShape.ActualWidth, _canvasWidth, out bool isHorizontallyCorrect);
        Canvas.SetLeft(_selectedShape, left);
        Canvas.SetLeft(_selectedShapeFrame, left);

        top = AdjustCoordinateWithinBounds(top, _selectedShape.ActualHeight, _canvasHeight, out bool isVerticallyCorrect);
        Canvas.SetTop(_selectedShape, top);
        Canvas.SetTop(_selectedShapeFrame, top);

        if (isHorizontallyCorrect || isVerticallyCorrect)
        {
            _startPoint = cursorPosition;
        }
    }
    
    private void AddShape(ShapeType type)
    {
        System.Windows.Shapes.Shape shape = type switch
        {
            ShapeType.Ellipse => ShapeFactory.GetEllipse(),
            ShapeType.Triangle => ShapeFactory.GetTriangle(),
            ShapeType.Rectangle => ShapeFactory.GetRectangle(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid shape type")
        };

        var offset = VisualTreeHelper.GetOffset(shape);
        
        Canvas.SetLeft(shape, offset.X);
        Canvas.SetTop(shape, offset.Y);
        
        Shapes.Add(shape);
    }

    private void RemoveShape()
    {
        if (_selectedShape != null && Shapes.Contains(_selectedShape) && _selectedShape != _selectedShapeFrame)
        {
            Shapes.Remove(_selectedShape);
            DeselectShape();
        }
    }
    
    private System.Windows.Shapes.Shape? GetShapeAtMousePosition(Point position)
    {
        return Shapes.Reverse().FirstOrDefault(shape => !(shape is Frame) && IsMouseOverShape(shape, position));
    }
    private bool IsMouseOverShape(System.Windows.Shapes.Shape shape, Point position)
    {
        var bounds = VisualTreeHelper.GetDescendantBounds(shape);
        var shapeTopLeft = new Point(Canvas.GetLeft(shape), Canvas.GetTop(shape));

        return bounds.Contains(new Point(position.X - shapeTopLeft.X, position.Y - shapeTopLeft.Y));
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}