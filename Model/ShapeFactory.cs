using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace shape_builder.Model;

public static class ShapeFactory
{
    public static Polygon GetTriangle()
    {
        PointCollection points =
        [
            new Point(75, 0),
            new Point(0, 50),
            new Point(150, 50)
        ];
        
        Polygon polygon = new Polygon
        {
            Points = new PointCollection(points),
            Fill = Brushes.Green,
            Stroke = Brushes.Black,
            StrokeThickness = 2,
            Uid = Guid.NewGuid().ToString()
        };
        Console.WriteLine($"{polygon.ActualHeight} {polygon.ActualWidth}");
        return polygon;
    }

    public static Ellipse GetEllipse()
    {
        Ellipse ellipse = new Ellipse
        {
            Width = 100,
            Height = 50,
            Fill = Brushes.Red,
            Stroke = Brushes.Black,
            StrokeThickness = 2,
            Uid = Guid.NewGuid().ToString()
        };
        
        return ellipse;
    }

    public static Rectangle GetRectangle()
    {
        Rectangle rectangle = new Rectangle
        {
            Width = 100,
            Height = 50,
            Fill = Brushes.Blue,
            Stroke = Brushes.Black,
            StrokeThickness = 2,
            Uid = Guid.NewGuid().ToString()
        };
        
        return rectangle;
    }
}