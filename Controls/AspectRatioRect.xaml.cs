using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using PsWinSnip.Models;

namespace PsWinSnip.Controls;

public partial class AspectRatioRect : System.Windows.Controls.UserControl
{
    private bool _isDragging;
    private System.Windows.Point _lastMousePosition;
    private SelectionRect? _model;

    public event EventHandler? SelectionChanged;

    public AspectRatioRect()
    {
        InitializeComponent();
        SelectionBorder.MouseLeftButtonDown += OnMouseDown;
        SelectionBorder.MouseMove += OnMouseMove;
        SelectionBorder.MouseLeftButtonUp += OnMouseUp;
        
        SizeChanged += (s, e) => UpdateHandlePositions();
    }

    public void Initialize(SelectionRect model)
    {
        _model = model;
        UpdateUI();
    }

    public void SetGridVisibility(bool visible)
    {
        GuidesGrid.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = true;
        _lastMousePosition = e.GetPosition(Parent as UIElement);
        SelectionBorder.CaptureMouse();
        e.Handled = true;
    }

    private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_isDragging || _model == null || Parent is not Canvas canvas) return;

        System.Windows.Point currentPos = e.GetPosition(canvas);
        double deltaX = currentPos.X - _lastMousePosition.X;
        double deltaY = currentPos.Y - _lastMousePosition.Y;

        Rect bounds = new Rect(0, 0, canvas.ActualWidth, canvas.ActualHeight);
        _model.Move(deltaX, deltaY, bounds);

        _lastMousePosition = currentPos;
        UpdateUI();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        SelectionBorder.ReleaseMouseCapture();
    }

    private void OnResizeDelta(object sender, DragDeltaEventArgs e)
    {
        if (_model == null || Parent is not Canvas canvas || sender is not Thumb thumb) return;

        ResizeDirection direction = Enum.Parse<ResizeDirection>(thumb.Tag.ToString()!);
        Rect bounds = new Rect(0, 0, canvas.ActualWidth, canvas.ActualHeight);

        bool isShiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        bool isAltDown = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);

        double newX = _model.X;
        double newY = _model.Y;
        double newWidth = _model.Width;
        double newHeight = _model.Height;

        double horizontalChange = e.HorizontalChange;
        double verticalChange = e.VerticalChange;

        if (isAltDown)
        {
            // Center anchored resizing: changes are doubled
            if (direction == ResizeDirection.Left || direction == ResizeDirection.Right || 
                direction == ResizeDirection.TopLeft || direction == ResizeDirection.TopRight ||
                direction == ResizeDirection.BottomLeft || direction == ResizeDirection.BottomRight)
            {
                newWidth += (direction == ResizeDirection.Left || direction == ResizeDirection.TopLeft || direction == ResizeDirection.BottomLeft) 
                    ? -horizontalChange * 2 : horizontalChange * 2;
            }

            if (direction == ResizeDirection.Top || direction == ResizeDirection.Bottom ||
                direction == ResizeDirection.TopLeft || direction == ResizeDirection.TopRight ||
                direction == ResizeDirection.BottomLeft || direction == ResizeDirection.BottomRight)
            {
                newHeight += (direction == ResizeDirection.Top || direction == ResizeDirection.TopLeft || direction == ResizeDirection.TopRight) 
                    ? -verticalChange * 2 : verticalChange * 2;
            }
        }
        else
        {
            // Standard anchored resizing
            switch (direction)
            {
                case ResizeDirection.Left:
                    newX += horizontalChange;
                    newWidth -= horizontalChange;
                    break;
                case ResizeDirection.Right:
                    newWidth += horizontalChange;
                    break;
                case ResizeDirection.Top:
                    newY += verticalChange;
                    newHeight -= verticalChange;
                    break;
                case ResizeDirection.Bottom:
                    newHeight += verticalChange;
                    break;
                case ResizeDirection.TopLeft:
                    newX += horizontalChange;
                    newWidth -= horizontalChange;
                    newY += verticalChange;
                    newHeight -= verticalChange;
                    break;
                case ResizeDirection.TopRight:
                    newWidth += horizontalChange;
                    newY += verticalChange;
                    newHeight -= verticalChange;
                    break;
                case ResizeDirection.BottomLeft:
                    newX += horizontalChange;
                    newWidth -= horizontalChange;
                    newHeight += verticalChange;
                    break;
                case ResizeDirection.BottomRight:
                    newWidth += horizontalChange;
                    newHeight += verticalChange;
                    break;
            }
        }

        var oldRatio = _model.AspectRatio;
        if (isShiftDown) _model.AspectRatio = null;

        _model.Resize(newX, newY, newWidth, newHeight, direction, bounds, isAltDown);
        
        if (isShiftDown) _model.AspectRatio = oldRatio;

        UpdateUI();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateUI()
    {
        if (_model == null) return;

        Canvas.SetLeft(this, _model.X);
        Canvas.SetTop(this, _model.Y);
        Width = Math.Max(0, _model.Width);
        Height = Math.Max(0, _model.Height);

        DimensionsText.Text = $"{(int)_model.Width} x {(int)_model.Height}";
    }

    private void UpdateHandlePositions()
    {
        // Center edge handles and stretch them slightly for better hit testing
        // HandleTop/Bottom
        double edgeWidth = Math.Max(0, Width - 20);
        HandleTop.Width = edgeWidth;
        HandleBottom.Width = edgeWidth;
        Canvas.SetLeft(HandleTop, 10);
        Canvas.SetLeft(HandleBottom, 10);

        // HandleLeft/Right
        double edgeHeight = Math.Max(0, Height - 20);
        HandleLeft.Height = edgeHeight;
        HandleRight.Height = edgeHeight;
        Canvas.SetTop(HandleLeft, 10);
        Canvas.SetTop(HandleRight, 10);
    }
}