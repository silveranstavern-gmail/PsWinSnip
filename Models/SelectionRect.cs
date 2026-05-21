using System.Windows;

namespace PsWinSnip.Models;

public class SelectionRect
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double? AspectRatio { get; set; }

    public const double MinSize = 200;

    public SelectionRect()
    {
        X = 100;
        Y = 100;
        Width = 400;
        Height = 400;
        AspectRatio = 1.0;
    }

    public SelectionRect(double x, double y, double width, double height, double? aspectRatio = null)
    {
        X = x;
        Y = y;
        Width = Math.Max(width, MinSize);
        Height = Math.Max(height, MinSize);
        AspectRatio = aspectRatio;

        if (AspectRatio.HasValue)
        {
            EnforceRatio(ResizeDirection.BottomRight);
        }
    }

    public void SetRatio(double? ratio)
    {
        AspectRatio = ratio;
        if (ratio.HasValue)
        {
            EnforceRatio(ResizeDirection.BottomRight);
        }
    }

    public void EnforceRatio(ResizeDirection direction)
    {
        if (!AspectRatio.HasValue) return;

        double ratio = AspectRatio.Value;

        switch (direction)
        {
            case ResizeDirection.Left:
            case ResizeDirection.Right:
                Height = Width / ratio;
                break;
            case ResizeDirection.Top:
            case ResizeDirection.Bottom:
                Width = Height * ratio;
                break;
            default: // Corners
                if (Width / Height > ratio)
                    Height = Width / ratio;
                else
                    Width = Height * ratio;
                break;
        }
    }

    public void Move(double deltaX, double deltaY, Rect bounds)
    {
        X += deltaX;
        Y += deltaY;

        if (X < bounds.Left) X = bounds.Left;
        if (Y < bounds.Top) Y = bounds.Top;
        if (X + Width > bounds.Right) X = bounds.Right - Width;
        if (Y + Height > bounds.Bottom) Y = bounds.Bottom - Height;
    }

    public void Resize(double newX, double newY, double newWidth, double newHeight, ResizeDirection direction, Rect bounds, bool isCenterAnchored = false)
    {
        double centerX = X + Width / 2;
        double centerY = Y + Height / 2;

        // Apply minimum size
        newWidth = Math.Max(newWidth, MinSize);
        newHeight = Math.Max(newHeight, MinSize);

        if (AspectRatio.HasValue)
        {
            double ratio = AspectRatio.Value;
            if (isCenterAnchored)
            {
                // Simple center-anchored ratio enforcement
                if (newWidth / newHeight > ratio) newHeight = newWidth / ratio;
                else newWidth = newHeight * ratio;
            }
            else
            {
                // Determine anchor point based on direction
                double anchorX = X, anchorY = Y;
                bool anchorRight = direction == ResizeDirection.Left || direction == ResizeDirection.TopLeft || direction == ResizeDirection.BottomLeft;
                bool anchorBottom = direction == ResizeDirection.Top || direction == ResizeDirection.TopLeft || direction == ResizeDirection.TopRight;

                if (anchorRight) anchorX = X + Width;
                if (anchorBottom) anchorY = Y + Height;

                // Adjust dimensions to ratio
                if (direction == ResizeDirection.Left || direction == ResizeDirection.Right)
                    newHeight = newWidth / ratio;
                else if (direction == ResizeDirection.Top || direction == ResizeDirection.Bottom)
                    newWidth = newHeight * ratio;
                else
                {
                    if (newWidth / newHeight > ratio) newHeight = newWidth / ratio;
                    else newWidth = newHeight * ratio;
                }

                // Reposition based on anchor
                if (anchorRight) newX = anchorX - newWidth;
                if (anchorBottom) newY = anchorY - newHeight;
            }
        }

        if (isCenterAnchored)
        {
            X = centerX - newWidth / 2;
            Y = centerY - newHeight / 2;
            Width = newWidth;
            Height = newHeight;
        }
        else
        {
            X = newX;
            Y = newY;
            Width = newWidth;
            Height = newHeight;
        }

        Constrain(bounds);
    }

    private void Constrain(Rect bounds)
    {
        if (Width > bounds.Width) Width = bounds.Width;
        if (Height > bounds.Height) Height = bounds.Height;

        if (X < bounds.Left) X = bounds.Left;
        if (Y < bounds.Top) Y = bounds.Top;
        if (X + Width > bounds.Right) X = bounds.Right - Width;
        if (Y + Height > bounds.Bottom) Y = bounds.Bottom - Height;
    }

    public Rect ToRect() => new Rect(X, Y, Width, Height);

    public SelectionRect Clone() => new SelectionRect(X, Y, Width, Height, AspectRatio);
}

public enum ResizeDirection
{
    None,
    Left,
    Right,
    Top,
    Bottom,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}