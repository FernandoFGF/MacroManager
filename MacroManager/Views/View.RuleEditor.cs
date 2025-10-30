using System;
using System.Drawing;
using System.Windows.Forms;
using MacroManager.Models;

namespace MacroManager
{
    public partial class View
    {
        private void CreateRuleEditorWithButtons(Control parent)
        {
            Panel ruleEditorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = _model.PanelBackColor
            };

            Label lblTitle = new Label
            {
                Text = "📝 Click on a line in the editor to see and modify action parameters",
                Dock = DockStyle.Top,
                Height = 60,
                Font = new Font("Courier New", 9, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor,
                Padding = new Padding(10, 5, 10, 5)
            };
            ruleEditorPanel.Controls.Add(lblTitle);

            Label lblKey = new Label
            {
                Text = "⌨️ Key:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Courier New", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor
            };
            ruleEditorPanel.Controls.Add(lblKey);

            TextBox txtKey = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Courier New", 10),
                Name = "txtKey",
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtKey.TextChanged += (s, e) => SaveActionChanges();
            ruleEditorPanel.Controls.Add(txtKey);

            Label lblDelay = new Label
            {
                Text = "⏱️ Delay (ms):",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Courier New", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor
            };
            ruleEditorPanel.Controls.Add(lblDelay);

            TextBox txtDelay = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Courier New", 10),
                Name = "txtDelay",
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtDelay.TextChanged += (s, e) => SaveActionChanges();
            ruleEditorPanel.Controls.Add(txtDelay);

            Label lblActionType = new Label
            {
                Text = "🎯 Action Type:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Courier New", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = _model.PanelForeColor,
                BackColor = _model.PanelBackColor
            };
            ruleEditorPanel.Controls.Add(lblActionType);

            ComboBox cmbActionType = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Courier New", 9),
                Name = "cmbActionType",
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                FlatStyle = FlatStyle.Flat
            };
            cmbActionType.Items.AddRange(new[] { "KeyPress", "KeyDown", "KeyUp", "MouseLeftDown", "MouseLeftUp", "MouseRightDown", "MouseRightUp", "MouseMove", "Delay" });
            cmbActionType.SelectedIndex = 0;
            cmbActionType.SelectedIndexChanged += (s, e) => SaveActionChanges();
            ruleEditorPanel.Controls.Add(cmbActionType);

            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = _model.PanelBackColor
            };

            Panel topButtonRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = _model.PanelBackColor
            };

            Button btnAdd = new Button
            {
                Text = "➕",
                Location = new Point(10, 5),
                Size = new Size(70, 35),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => _controller.AddNewAction();
            ApplyRetroButtonStyle(btnAdd, _model.AccentColor, _model.BorderColor);
            topButtonRow.Controls.Add(btnAdd);

            Button btnRemove = new Button
            {
                Text = "➖",
                Location = new Point(85, 5),
                Size = new Size(70, 35),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += (s, e) =>
            {
                if (_selectedActionIndices.Count > 1)
                {
                    _controller.DeleteActions(_selectedActionIndices);
                }
                else
                {
                    _controller.DeleteAction(_selectedActionIndex);
                }
            };
            ApplyRetroButtonStyle(btnRemove, Color.FromArgb(150, 30, 30), _model.BorderColor);
            topButtonRow.Controls.Add(btnRemove);

            Button btnDuplicate = new Button
            {
                Text = "📋",
                Location = new Point(160, 5),
                Size = new Size(70, 35),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDuplicate.FlatAppearance.BorderSize = 0;
            btnDuplicate.Click += (s, e) =>
            {
                if (_selectedActionIndices.Count > 1)
                {
                    _controller.DuplicateActions(_selectedActionIndices);
                }
                else
                {
                    _controller.DuplicateAction(_selectedActionIndex);
                }
            };
            ApplyRetroButtonStyle(btnDuplicate, _model.AccentColor, _model.BorderColor);
            topButtonRow.Controls.Add(btnDuplicate);

            buttonPanel.Controls.Add(topButtonRow);

            Panel recordingRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = _model.PanelBackColor
            };

            _btnRecord = new Button
            {
                Text = "🔴",
                Location = new Point(10, 5),
                Size = new Size(70, 35),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnRecord.FlatAppearance.BorderSize = 0;
            _btnRecord.Click += (s, e) => _controller.StartRecording();
            ApplyRetroButtonStyle(_btnRecord, Color.FromArgb(150, 30, 30), _model.BorderColor);
            recordingRow.Controls.Add(_btnRecord);

            _btnStopRecord = new Button
            {
                Text = "⏹️",
                Location = new Point(85, 5),
                Size = new Size(70, 35),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnStopRecord.FlatAppearance.BorderSize = 0;
            _btnStopRecord.Click += (s, e) => _controller.StopRecording();
            ApplyRetroButtonStyle(_btnStopRecord, Color.FromArgb(60, 60, 60), _model.BorderColor);
            recordingRow.Controls.Add(_btnStopRecord);

            Button btnSave = new Button
            {
                Text = "💾",
                Location = new Point(160, 5),
                Size = new Size(70, 35),
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => _controller.SaveCurrentMacro();
            ApplyRetroButtonStyle(btnSave, _model.AccentColor, _model.BorderColor);
            recordingRow.Controls.Add(btnSave);

            buttonPanel.Controls.Add(recordingRow);
            ruleEditorPanel.Controls.Add(buttonPanel);

            _txtKey = txtKey;
            _txtDelay = txtDelay;
            _cmbActionType = cmbActionType;

            parent.Controls.Add(ruleEditorPanel);
        }

        private void LoadActionToEditor(int actionIndex)
        {
            if (_model.CurrentMacro == null || actionIndex < 0 || actionIndex >= _model.CurrentMacro.Actions.Count)
            {
                ClearRuleEditor();
                return;
            }

            _selectedActionIndex = actionIndex;
            var action = _model.CurrentMacro.Actions[actionIndex];

            _txtKey.Text = _model.GetKeyDisplay(action);
            _txtDelay.Text = action.DelayMs.ToString();
            _cmbActionType.SelectedItem = action.Type.ToString();
        }

        private void ClearRuleEditor()
        {
            _selectedActionIndex = -1;
            _txtKey.Text = "";
            _txtDelay.Text = "0";
            _cmbActionType.SelectedIndex = 0;
        }

        private void SaveActionChanges()
        {
            if (_selectedActionIndex < 0 || _model.CurrentMacro == null || _selectedActionIndex >= _model.CurrentMacro.Actions.Count)
                return;

            if (int.TryParse(_txtDelay.Text, out int delay))
            {
                ActionType actionType = ActionType.KeyPress;
                if (_cmbActionType.SelectedItem != null)
                {
                    Enum.TryParse<ActionType>(_cmbActionType.SelectedItem.ToString(), out actionType);
                }

                _controller.UpdateAction(_selectedActionIndex, _txtKey.Text, delay, actionType);
            }
        }
    }
}


