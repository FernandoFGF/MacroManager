using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacroManager.Models;

namespace MacroManager
{
    public partial class View
    {
        private void CreateTextEditorWithSwitch(Control parent)
        {
            _actionsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _model.CardBackColor,
                Padding = new Padding(10),
                AutoScroll = true
            };
            // Usamos AutoScroll del panel; no añadimos un VScrollBar manual
            _actionsPanel.MouseDown += OnActionsPanelMouseDown;
            _actionsPanel.MouseMove += OnActionsPanelMouseMove;
            _actionsPanel.MouseUp += OnActionsPanelMouseUp;
            _actionsPanel.Paint += OnActionsPanelPaint;
            _actionsPanel.Resize += (s, e) => AdjustActionButtonsWidth();
            parent.Controls.Add(_actionsPanel);
        }

        private void RefreshActionsDisplay()
        {
            if (_actionsPanel == null || _model.CurrentMacro == null) return;

            _actionsPanel.Controls.Clear();

            int yPosition = 10;
            int buttonHeight = 70;
            int buttonSpacing = 15;

            for (int i = 0; i < _model.CurrentMacro.Actions.Count; i++)
            {
                var action = _model.CurrentMacro.Actions[i];
                var actionButton = CreateActionButton(action, i, yPosition);
                _actionsPanel.Controls.Add(actionButton);
                yPosition += buttonHeight + buttonSpacing;
            }

            _actionsPanel.Height = Math.Max(_actionsPanel.Parent.Height, yPosition + 20);
            AdjustActionButtonsWidth();
            UpdateSelectionHighlight();
        }

        private void AdjustActionButtonsWidth()
        {
            if (_actionsPanel == null) return;
            int usableWidth = Math.Max(0, _actionsPanel.ClientSize.Width - 20);
            foreach (Control control in _actionsPanel.Controls)
            {
                if (control is Button btn)
                {
                    btn.Width = usableWidth;
                }
            }
        }

        private void ApplyRetroButtonStyle(Button button, Color accentColor, Color borderColor)
        {
            bool isPressed = false;

            button.MouseEnter += (s, e) =>
            {
                if (!isPressed)
                {
                    button.BackColor = Color.FromArgb(30, accentColor.R, accentColor.G, accentColor.B);
                }
            };

            button.MouseLeave += (s, e) =>
            {
                if (!isPressed)
                {
                    button.BackColor = _model.CardBackColor;
                }
            };

            button.MouseDown += (s, e) =>
            {
                isPressed = true;
                button.BackColor = Color.FromArgb(60, accentColor.R, accentColor.G, accentColor.B);
                button.Invalidate();
            };

            button.MouseUp += (s, e) =>
            {
                isPressed = false;
                button.BackColor = Color.FromArgb(30, accentColor.R, accentColor.G, accentColor.B);
                button.Invalidate();
            };

            button.Paint += (s, e) =>
            {
                var g = e.Graphics;
                var rect = new Rectangle(0, 0, button.Width - 1, button.Height - 1);

                using (var borderPen = new Pen(borderColor))
                {
                    g.DrawRectangle(borderPen, rect);
                }

                Color light = Color.FromArgb(Math.Min(255, borderColor.R + 60), Math.Min(255, borderColor.G + 60), Math.Min(255, borderColor.B + 60));
                Color dark = Color.FromArgb(Math.Max(0, borderColor.R - 60), Math.Max(0, borderColor.G - 60), Math.Max(0, borderColor.B - 60));

                using (var penTopLeft = new Pen(isPressed ? dark : light))
                using (var penBottomRight = new Pen(isPressed ? light : dark))
                {
                    g.DrawLine(penTopLeft, 1, 1, rect.Width - 1, 1);
                    g.DrawLine(penTopLeft, 1, 1, 1, rect.Height - 1);
                    g.DrawLine(penBottomRight, 1, rect.Height - 1, rect.Width - 1, rect.Height - 1);
                    g.DrawLine(penBottomRight, rect.Width - 1, 1, rect.Width - 1, rect.Height - 1);
                }
            };
        }

        private Button CreateActionButton(MacroAction action, int index, int yPosition)
        {
            string keyDisplay = _model.GetKeyDisplay(action);
            string actionType = _model.GetActionTypeDisplay(action.Type);
            
            var button = new Button
            {
                Text = $"#{index + 1}  {actionType}\nKey: {keyDisplay}\nDelay: {action.DelayMs}ms",
                Location = new Point(10, yPosition),
                Size = new Size(Math.Max(0, _actionsPanel.ClientSize.Width - 20), 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Tag = index,
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderColor = _model.AccentColor;
            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, _model.AccentColor.R, _model.AccentColor.G, _model.AccentColor.B);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, _model.AccentColor.R, _model.AccentColor.G, _model.AccentColor.B);

            button.MouseEnter += (s, e) => {
                if (button.BackColor != _model.AccentColor)
                {
                    button.BackColor = Color.FromArgb(30, _model.AccentColor.R, _model.AccentColor.G, _model.AccentColor.B);
                }
            };
            
            button.MouseLeave += (s, e) => {
                if (button.BackColor != _model.AccentColor)
                {
                    button.BackColor = _model.CardBackColor;
                }
            };

            button.MouseDown += (s, e) => HandleActionButtonMouseDown(index, e, (Control)s);
            button.MouseMove += (s, e) => HandleChildMouseMove((Control)s, e);
            button.MouseUp += (s, e) => HandleChildMouseUp((Control)s, e);

            return button;
        }

        private void SelectAction(int actionIndex)
        {
            if (_model.CurrentMacro == null || actionIndex < 0 || actionIndex >= _model.CurrentMacro.Actions.Count)
                return;

            _selectedActionIndex = actionIndex;
            _selectedActionIndices.Clear();
            _selectedActionIndices.Add(actionIndex);
            UpdateSelectionHighlight();
            LoadActionToEditor(actionIndex);
        }

        private void HandleActionButtonMouseDown(int index, MouseEventArgs e, Control sourceControl)
        {
            bool ctrl = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            bool shift = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

            if (shift && _selectedActionIndex >= 0)
            {
                SelectRange(_selectedActionIndex, index);
            }
            else if (ctrl)
            {
                ToggleSelection(index);
                _selectedActionIndex = index;
            }
            else
            {
                _pendingClick = true;
                _pendingClickIndex = index;
                Point screen = sourceControl.PointToScreen(e.Location);
                _pendingClickStartPoint = _actionsPanel.PointToClient(screen);
            }

            UpdateEditorForSelection();
        }

        private void HandleChildMouseMove(Control child, MouseEventArgs e)
        {
            Point screen = child.PointToScreen(e.Location);
            Point pt = _actionsPanel.PointToClient(screen);

            if (_pendingClick && e.Button == MouseButtons.Left && Distance(pt, _pendingClickStartPoint) > 4 &&
                !(Control.ModifierKeys.HasFlag(Keys.Control) || Control.ModifierKeys.HasFlag(Keys.Shift)))
            {
                _isDraggingSelection = true;
                _dragStartPoint = _pendingClickStartPoint;
                _dragSelectionRect = Rectangle.Empty;
                _pendingClick = false;
                _actionsPanel.Capture = true;
            }

            if (_isDraggingSelection)
            {
                UpdateDragRectangle(pt);
            }
        }

        private void HandleChildMouseUp(Control child, MouseEventArgs e)
        {
            Point screen = child.PointToScreen(e.Location);
            Point pt = _actionsPanel.PointToClient(screen);

            if (_isDraggingSelection)
            {
                OnActionsPanelMouseUp(_actionsPanel, new MouseEventArgs(e.Button, e.Clicks, pt.X, pt.Y, e.Delta));
                return;
            }

            if (_pendingClick)
            {
                _pendingClick = false;
                SetSingleSelection(_pendingClickIndex);
            }
        }

        private int Distance(Point a, Point b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return (int)Math.Sqrt(dx * dx + dy * dy);
        }

        private void SetSingleSelection(int index)
        {
            _selectedActionIndices.Clear();
            if (index >= 0) _selectedActionIndices.Add(index);
            _selectedActionIndex = index;
            UpdateSelectionHighlight();
            UpdateEditorForSelection();
        }

        private void ToggleSelection(int index)
        {
            if (_selectedActionIndices.Contains(index))
                _selectedActionIndices.Remove(index);
            else
                _selectedActionIndices.Add(index);
            UpdateSelectionHighlight();
            UpdateEditorForSelection();
        }

        private void SelectRange(int anchor, int end)
        {
            if (anchor < 0) { SetSingleSelection(end); return; }
            int start = Math.Min(anchor, end);
            int last = Math.Max(anchor, end);
            _selectedActionIndices = Enumerable.Range(start, last - start + 1).ToList();
            _selectedActionIndex = end;
            UpdateSelectionHighlight();
            UpdateEditorForSelection();
        }

        private void SelectByRectangle(Rectangle rect)
        {
            List<int> hits = new List<int>();
            foreach (Control control in _actionsPanel.Controls)
            {
                if (control is Button button && button.Tag is int idx)
                {
                    if (rect.IntersectsWith(button.Bounds))
                        hits.Add(idx);
                }
            }
            _selectedActionIndices = hits;
            if (hits.Count > 0) _selectedActionIndex = hits.Last();
            UpdateSelectionHighlight();
            UpdateEditorForSelection();
        }

        private void UpdateSelectionHighlight()
        {
            foreach (Control control in _actionsPanel.Controls)
            {
                if (control is Button button && button.Tag is int index)
                {
                    if (_selectedActionIndices.Contains(index))
                    {
                        button.BackColor = _model.AccentColor;
                        button.ForeColor = Color.White;
                    }
                    else
                    {
                        button.BackColor = _model.CardBackColor;
                        button.ForeColor = _model.PanelForeColor;
                    }
                }
            }
            _actionsPanel.Invalidate();
        }

        private void OnActionsPanelMouseDown(object sender, MouseEventArgs e)
        {
            var hit = _actionsPanel.GetChildAtPoint(e.Location);
            if (hit == null || !(hit is Button))
            {
                _isDraggingSelection = true;
                _dragStartPoint = e.Location;
                _dragSelectionRect = new Rectangle(e.Location, Size.Empty);
                _actionsPanel.Capture = true;
            }
        }

        private void OnActionsPanelMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingSelection) return;
            UpdateDragRectangle(e.Location);
        }

        private void UpdateDragRectangle(Point current)
        {
            int x = Math.Min(_dragStartPoint.X, current.X);
            int y = Math.Min(_dragStartPoint.Y, current.Y);
            int w = Math.Abs(current.X - _dragStartPoint.X);
            int h = Math.Abs(current.Y - _dragStartPoint.Y);
            _dragSelectionRect = new Rectangle(x, y, w, h);
            _actionsPanel.Invalidate();
        }

        private void OnActionsPanelMouseUp(object sender, MouseEventArgs e)
        {
            if (!_isDraggingSelection) return;
            _isDraggingSelection = false;
            _actionsPanel.Capture = false;
            if (_dragSelectionRect.Width > 2 && _dragSelectionRect.Height > 2)
            {
                bool ctrl = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                if (ctrl)
                {
                    List<int> prev = new List<int>(_selectedActionIndices);
                    SelectByRectangle(_dragSelectionRect);
                    _selectedActionIndices = prev.Union(_selectedActionIndices).ToList();
                    UpdateSelectionHighlight();
                    UpdateEditorForSelection();
                }
                else
                {
                    SelectByRectangle(_dragSelectionRect);
                }
            }
            else
            {
                SetSingleSelection(-1);
            }
            _dragSelectionRect = Rectangle.Empty;
            _actionsPanel.Invalidate();
        }

        private void OnActionsPanelPaint(object sender, PaintEventArgs e)
        {
            if (_dragSelectionRect != Rectangle.Empty)
            {
                using (var brush = new SolidBrush(Color.FromArgb(40, _model.AccentColor)))
                using (var pen = new Pen(_model.AccentColor, 1))
                {
                    e.Graphics.FillRectangle(brush, _dragSelectionRect);
                    e.Graphics.DrawRectangle(pen, _dragSelectionRect);
                }
            }
        }

        private void UpdateEditorForSelection()
        {
            if (_selectedActionIndices.Count == 1)
            {
                LoadActionToEditor(_selectedActionIndices[0]);
            }
            else
            {
                ClearRuleEditor();
            }
        }

        private async Task TogglePlayPauseStop()
        {
            int repeatCount = (int)_numLoopCount.Value;
            await _controller.TogglePlayPauseStop(repeatCount);
        }
    }
}


