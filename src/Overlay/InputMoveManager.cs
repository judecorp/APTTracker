using Hearthstone_Deck_Tracker;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using Core = Hearthstone_Deck_Tracker.API.Core;
using DrawingPoint = System.Drawing.Point;

namespace APTTracker.Overlay
{
    public class InputMoveManager
    {
        private User32.MouseInput _mouseInput;
        private bool _selected = false;
        private bool _hasMoved;
        private readonly FrameworkElement _element;
        private double _offsetX;
        private double _offsetY;
        private bool _isEnabled = false;

        public InputMoveManager(FrameworkElement element)
        {
            _element = element;
        }

        private Canvas GetParentCanvas()
        {
            if (_element == null)
                return Core.OverlayCanvas;
            
            var parent = _element.Parent;
            while (parent != null)
            {
                if (parent is Canvas canvas)
                    return canvas;
                if (parent is FrameworkElement fe)
                    parent = fe.Parent;
                else
                    break;
            }
            
            return Core.OverlayCanvas;
        }

        private void MouseInputOnLmbDown(object sender, EventArgs eventArgs)
        {
            if (!_isEnabled || _selected)
                return;

            try
            {
                var pos = User32.GetMousePos();
                var mousePos = new DrawingPoint(pos.X, pos.Y);
                if (PointInsideControl(mousePos, _element))
                {
                    try
                    {
                        var canvas = GetParentCanvas();
                        if (canvas != null)
                        {
                            var canvasPos = canvas.PointFromScreen(new System.Windows.Point(pos.X, pos.Y));
                            _offsetX = canvasPos.X - Canvas.GetLeft(_element);
                            _offsetY = canvasPos.Y - Canvas.GetTop(_element);
                            PluginPosition.BeginDrag(_element);
                            _hasMoved = false;
                            _selected = true;
                        }
                    }
                    catch
                    {
                        _selected = false;
                    }
                }
                else
                {
                    _selected = false;
                }
            }
            catch (Exception)
            {
                _selected = false;
            }
        }

        private void MouseInputOnLmbUp(object sender, EventArgs eventArgs)
        {
            FinishDrag();
        }

        private void FinishDrag()
        {
            if (!_selected)
                return;

            try
            {
                if (_hasMoved)
                {
                    // Account for a final size change even if the pointer has stopped.
                    PluginPosition.SetPosition(_element, Canvas.GetLeft(_element), Canvas.GetTop(_element));
                    PluginPosition.SavePosition(_element);
                }
            }
            finally
            {
                _selected = false;
                _hasMoved = false;
                PluginPosition.EndDrag(_element);
                PluginPosition.RestorePosition(_element);
            }
        }

        private void MouseInputOnMouseMoved(object sender, EventArgs eventArgs)
        {
            if (!_isEnabled || !_selected)
                return;

            try
            {
                var pos = User32.GetMousePos();
                try
                {
                    var canvas = GetParentCanvas();
                    if (canvas != null)
                    {
                        var canvasPos = canvas.PointFromScreen(new System.Windows.Point(pos.X, pos.Y));
                        var previousLeft = Canvas.GetLeft(_element);
                        var previousTop = Canvas.GetTop(_element);
                        PluginPosition.SetPosition(_element, canvasPos.X - _offsetX, canvasPos.Y - _offsetY);
                        _hasMoved |= Canvas.GetLeft(_element) != previousLeft || Canvas.GetTop(_element) != previousTop;
                    }
                }
                catch
                {
                    // Ignore coordinate conversion errors
                }
            }
            catch (Exception)
            {
            }
        }

        private bool PointInsideControl(DrawingPoint p, FrameworkElement control)
        {
            try
            {
                if (!control.IsVisible || !control.IsHitTestVisible)
                    return false;

                var pos = control.PointFromScreen(new System.Windows.Point(p.X, p.Y));
                // Basic bounds check
                if (pos.X < 0 || pos.X > control.ActualWidth || pos.Y < 0 || pos.Y > control.ActualHeight)
                    return false;

                // Use the input hit test for both content and visual elements. A raw
                // visual hit test can return controls in collapsed/inactive panels.
                var input = control.InputHitTest(pos) as DependencyObject;
                while (input != null)
                {
                    if (input is System.Windows.Documents.Hyperlink ||
                        input is ButtonBase || input is Slider || input is Thumb || input is ScrollBar)
                        return false;

                    if (input == control)
                        break;

                    input = input is ContentElement content
                        ? ContentOperations.GetParent(content) ?? (content as FrameworkContentElement)?.Parent
                        : VisualTreeHelper.GetParent(input);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Toggle()
        {
            if (_mouseInput == null)
            {
                _mouseInput = new User32.MouseInput();
                _mouseInput.LmbDown += MouseInputOnLmbDown;
                _mouseInput.LmbUp += MouseInputOnLmbUp;
                _mouseInput.MouseMoved += MouseInputOnMouseMoved;
                _isEnabled = true;
                return true;
            }
            Dispose();
            return false;
        }

        public void Enable()
        {
            if (_mouseInput == null)
            {
                _mouseInput = new User32.MouseInput();
                _mouseInput.LmbDown += MouseInputOnLmbDown;
                _mouseInput.LmbUp += MouseInputOnLmbUp;
                _mouseInput.MouseMoved += MouseInputOnMouseMoved;
            }
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
            FinishDrag();
        }

        public void Dispose()
        {
            _isEnabled = false;
            FinishDrag();
            _mouseInput?.Dispose();
            _mouseInput = null;
        }
    }
}
