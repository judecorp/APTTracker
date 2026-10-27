using Hearthstone_Deck_Tracker;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using Core = Hearthstone_Deck_Tracker.API.Core;
using DrawingPoint = System.Drawing.Point;

namespace APTTracker.Logic
{
    public class InputMoveManager
    {
        private User32.MouseInput _mouseInput;
        private bool _selected = false;
        private FrameworkElement _element;
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
            if (!_isEnabled)
                return;

            try
            {
                var pos = User32.GetMousePos();
                var mousePos = new DrawingPoint(pos.X, pos.Y);
                if (PointInsideControl(mousePos, _element))
                {
                    _selected = true;
                    try
                    {
                        var canvas = GetParentCanvas();
                        if (canvas != null)
                        {
                            var canvasPos = canvas.PointFromScreen(new System.Windows.Point(pos.X, pos.Y));
                            _offsetX = canvasPos.X - Canvas.GetLeft(_element);
                            _offsetY = canvasPos.Y - Canvas.GetTop(_element);
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[InputMoveManager] LmbDown error: {ex.Message}");
                _selected = false;
            }
        }

        private void MouseInputOnLmbUp(object sender, EventArgs eventArgs)
        {
            if (_selected && _isEnabled && _element != null)
            {
                PluginPosition.SavePosition(_element);
            }
            _selected = false;
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
                        PluginPosition.SetPosition(_element, canvasPos.X - _offsetX, canvasPos.Y - _offsetY);
                    }
                }
                catch
                {
                    // Ignore coordinate conversion errors
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[InputMoveManager] MouseMoved error: {ex.Message}");
            }
        }

        private bool PointInsideControl(DrawingPoint p, FrameworkElement control)
        {
            try
            {
                var pos = control.PointFromScreen(new System.Windows.Point(p.X, p.Y));
                // Basic bounds check
                if (pos.X < 0 || pos.X > control.ActualWidth || pos.Y < 0 || pos.Y > control.ActualHeight)
                    return false;

                // Hit‑test the point inside the element
                var hit = System.Windows.Media.VisualTreeHelper.HitTest(control, new System.Windows.Point(pos.X, pos.Y))?.VisualHit;

                // Walk up the visual tree to detect Buttons, ScrollBar parts, etc.
                while (hit != null)
                {
                    if (hit is System.Windows.Controls.Primitives.ButtonBase ||
                        hit is Slider ||
                        hit is System.Windows.Controls.Primitives.Thumb ||
                         hit is System.Windows.Controls.Primitives.ScrollBar)
                        return false; // exempt from dragging
                    hit = System.Windows.Media.VisualTreeHelper.GetParent(hit) as System.Windows.DependencyObject;
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
            _selected = false;
        }

        public void Dispose()
        {
            _isEnabled = false;
            _selected = false;
            _mouseInput?.Dispose();
            _mouseInput = null;
        }
    }
}
