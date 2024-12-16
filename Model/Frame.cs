using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace shape_builder.Model;

public class Frame : Shape
{
    private readonly double _canvasWidth;
    private readonly double _canvasHeight;

    private Shape? _selectedShape;
    
    private double _x;
    private double _y;
    
    private bool _isResizing;
    
    private static readonly double MarkerRadius = 5;
    
    private Corner _corner = Corner.None;
    
    private Point _oppositePoint;

    public Frame(double canvasWidth, double canvasHeight)
    {
        _canvasWidth = canvasWidth;
        _canvasHeight = canvasHeight;
    }

    public static readonly DependencyProperty WidthProperty =
        DependencyProperty.Register(
            nameof(Width),
            typeof(double),
            typeof(Frame),
            new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty HeightProperty =
        DependencyProperty.Register(
            nameof(Height),
            typeof(double),
            typeof(Frame),
            new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FillColorProperty =
        DependencyProperty.Register(
            nameof(FillColor),
            typeof(Brush),
            typeof(Frame),
            new FrameworkPropertyMetadata(Brushes.Blue, FrameworkPropertyMetadataOptions.AffectsRender));

    public double Width
    {
        get => (double)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public double Height
    {
        get => (double)GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    public Brush FillColor
    {
        get => (Brush)GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }

    protected override Geometry DefiningGeometry
    {
        get => new RectangleGeometry(new Rect(0, 0, Width, Height));
    }

    private void InitializeByShape(Shape shape)
    {
        Canvas.SetLeft(this, Canvas.GetLeft(shape));
        Canvas.SetTop(this, Canvas.GetTop(shape));
        
        Width = shape.ActualWidth;
        Height = shape.ActualHeight;
    }

    public void SelectShape(Shape shape)
    {
        _selectedShape = shape;
        
        InitializeByShape(shape);
    }

    public void DeselectShape()
    {
        _selectedShape = null;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        var borderPen = new Pen(Brushes.Black, 0.5);
        drawingContext.DrawRectangle(null, borderPen, new Rect(0, 0, Width, Height));

        DrawCornerMarkers(drawingContext);
    }

    private void DrawCornerMarkers(DrawingContext drawingContext)
    {
        drawingContext.DrawEllipse(Brushes.White, null, new Point(_x, _y), MarkerRadius, MarkerRadius);
        drawingContext.DrawEllipse(Brushes.White, null, new Point(_x + Width, _y), MarkerRadius, MarkerRadius);
        drawingContext.DrawEllipse(Brushes.White, null, new Point(_x, _y + Height), MarkerRadius, MarkerRadius);
        drawingContext.DrawEllipse(Brushes.White, null, new Point(_x + Width, _y + Height), MarkerRadius, MarkerRadius);
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        SetCursor(e.GetPosition(this));
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        var position = e.GetPosition(this);

        if (!IsMouseOverCorner(position)) return;
        
        _isResizing = true;
        Mouse.Capture(this);
        e.Handled = true;
        DetermineCorner(position);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        _corner = Corner.None;
        _isResizing = false;
        Mouse.Capture(null);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_isResizing)
        {
            HandleResize(e.GetPosition(this));
            ResizeSelectedShape();
        }
        else
        {
            SetCursor(e.GetPosition(this));
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        Cursor = Cursors.Arrow;
    }

    private void DetermineCorner(Point position)
    {
        if (IsMouseOverLeftTopCorner(position)) _corner = Corner.LeftTop;
        else if (IsMouseOverRightTopCorner(position)) _corner = Corner.RightTop;
        else if (IsMouseOverLeftBottomCorner(position)) _corner = Corner.LeftBottom;
        else if (IsMouseOverRightBottomCorner(position)) _corner = Corner.RightBottom;

        SetOppositePoint();
    }

    private void SetOppositePoint()
    {
        var left = Canvas.GetLeft(this);
        var top = Canvas.GetTop(this);

        _oppositePoint = _corner switch
        {
            Corner.LeftTop => new Point(left + Width, top + Height),
            Corner.RightTop => new Point(left, top + Height),
            Corner.LeftBottom => new Point(left + Width, top),
            Corner.RightBottom => new Point(left, top),
            _ => _oppositePoint
        };
    }

    private void HandleResize(Point cursorPosition)
    {
        switch (_corner)
        {
            case Corner.LeftTop:
                ResizeFromLeftTop(cursorPosition);
                break;
            case Corner.RightTop:
                ResizeFromRightTop(cursorPosition);
                break;
            case Corner.LeftBottom:
                ResizeFromLeftBottom(cursorPosition);
                break;
            case Corner.RightBottom:
                ResizeFromRightBottom(cursorPosition);
                break;
        }
    }

    private void ResizeFromLeftTop(Point delta)
    {
        Resize(-delta.X, -delta.Y);
        UpdatePosition(delta.X, delta.Y);
    }

    private void ResizeFromRightTop(Point delta)
    {
        Resize(delta.X - Width, -delta.Y);
        UpdatePosition(null, delta.Y);
    }

    private void ResizeFromLeftBottom(Point delta)
    {
        Resize(-delta.X, delta.Y - Height);
        UpdatePosition(delta.X, null);
    }

    private void ResizeFromRightBottom(Point delta)
    {
        Resize(delta.X - Width, delta.Y - Height);
    }

    private void Resize(double deltaX, double deltaY)
    {
        if (IsResizingAcceptable(Canvas.GetLeft(this)))
        {
            if (IsDimensionOutOfRange(Width, deltaX, _canvasWidth, Canvas.GetLeft(this)))
            {
                Width = Math.Max(0, Math.Min(_canvasWidth, Width + deltaX));
            }
            else if (Canvas.GetLeft(this) > 0)
            {
                Width = _canvasWidth - Canvas.GetLeft(this);
            }
        }

        if (!IsResizingAcceptable(Canvas.GetTop(this))) return;
        if (IsDimensionOutOfRange(Height, deltaY, _canvasHeight, Canvas.GetTop(this)))
        {
            Height = Math.Max(0, Math.Min(_canvasHeight, Height + deltaY));
        }
        else if (Canvas.GetTop(this) > 0)
        {
            Height = _canvasHeight - Canvas.GetTop(this);
        }
    }

    private void UpdatePosition(double? deltaX, double? deltaY)
    {
        double deltaXValue = deltaX ?? 0;
        double deltaYValue = deltaY ?? 0;
        
        var left = Canvas.GetLeft(this) + deltaXValue;
        var top = Canvas.GetTop(this) + deltaYValue;

        if (left < 0)
        {
            Canvas.SetLeft(this, 0);
        }
        else if (left < _oppositePoint.X)
        {
            Canvas.SetLeft(this, left);
        }
        else if (left > _oppositePoint.X)
        {
            Canvas.SetLeft(this, _oppositePoint.X);
        }

        if (top < 0)
        {
            Canvas.SetTop(this, 0);
        }
        else if (top < _oppositePoint.Y)
        {
            Canvas.SetTop(this, top);
        }
        else if (top > _oppositePoint.Y)
        {
            Canvas.SetTop(this, _oppositePoint.Y);
        }
    }

    private void ResizeSelectedShape()
    {
        if (_selectedShape == null) return;

        _selectedShape.Width = Width;
        _selectedShape.Height = Height;

        var left = Canvas.GetLeft(this);
        var top = Canvas.GetTop(this);

        Canvas.SetLeft(_selectedShape, left);
        Canvas.SetTop(_selectedShape, top);

        if (_selectedShape is Polygon)
        {
            ResizeTriangle();
        }
    }

    private void ResizeTriangle()
    {
        if (_selectedShape is Polygon)
        {
            Polygon updatedPolygon = (_selectedShape as Polygon)!;
            PointCollection points =
            [
                new Point(0, Height),
                new Point(Width / 2, 0),
                new Point(Width, Height)
            ];
            updatedPolygon.Points = points;
            
            _selectedShape = updatedPolygon;
        }
    }

    private void SetCursor(Point position)
    {
        Cursor = IsMouseOverCorner(position) ? IsMouseOverLeftTopCorner(position) || IsMouseOverRightBottomCorner(position)
            ? Cursors.SizeNWSE : Cursors.SizeNESW
            : Cursors.Arrow;
    }

    private bool IsResizingAcceptable(double dimension)
    {
        switch (_corner)
        {
            case Corner.LeftTop:
            case Corner.RightTop:
            case Corner.LeftBottom:
                return dimension != 0;
            case Corner.RightBottom:
                return true;
            case Corner.None:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool IsDimensionOutOfRange(double currentDimension, double delta, double maxDimension, double positionParam)
    {
        return currentDimension + delta + positionParam < maxDimension;
    }

    private bool IsMouseOverLeftTopCorner(Point position) => position.X < MarkerRadius && position.Y < MarkerRadius;

    private bool IsMouseOverRightTopCorner(Point position) => position.X > Width - MarkerRadius && position.Y < MarkerRadius;

    private bool IsMouseOverLeftBottomCorner(Point position) => position.X < MarkerRadius && position.Y > Height - MarkerRadius;

    private bool IsMouseOverRightBottomCorner(Point position) => position.X > Width - MarkerRadius && position.Y > Height - MarkerRadius;

    private bool IsMouseOverCorner(Point position) =>
        IsMouseOverLeftTopCorner(position) || IsMouseOverRightTopCorner(position) ||
        IsMouseOverLeftBottomCorner(position) || IsMouseOverRightBottomCorner(position);

    private enum Corner
    {
        LeftTop,
        RightTop,
        LeftBottom,
        RightBottom,
        None
    }
}