using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MacroManager.Models;

namespace MacroManager
{
    public partial class View
    {
        private void CreateMacroTree(Control parent)
        {
            _macroTreeView = new TreeView
            {
                Dock = DockStyle.Fill,
                Nodes = { },
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                BorderStyle = BorderStyle.None,
                Font = _model.CreateFont(10),
                Indent = 20,
                ShowLines = false,
                ShowPlusMinus = false,
                ShowRootLines = false,
                FullRowSelect = true,
                HotTracking = true
            };

            CreateTreeViewContextMenu();
            RefreshMacroTree();

            _macroTreeView.NodeMouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && e.Node.Tag is MacroConfig macro)
                {
                    _controller.LoadMacro(macro);
                }
            };

            _macroTreeView.DoubleClick += (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig)
                {
                    _controller.RenameCurrentMacro();
                }
            };

            _macroTreeView.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete && _macroTreeView.SelectedNode?.Tag is MacroConfig)
                {
                    _controller.DeleteCurrentMacro();
                }
            };

            parent.Controls.Add(_macroTreeView);
        }

        private void CreateTreeViewContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip
            {
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = _model.CreateFont(9)
            };

            ToolStripMenuItem renameItem = new ToolStripMenuItem("✏️ Rename", null, (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig)
                {
                    _controller.RenameCurrentMacro();
                }
            });
            contextMenu.Items.Add(renameItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem duplicateItem = new ToolStripMenuItem("📋 Duplicate", null, (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig)
                {
                    _controller.DuplicateCurrentMacro();
                }
            });
            contextMenu.Items.Add(duplicateItem);

            // Export removed

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem openLocationItem = new ToolStripMenuItem("📁 Open Location", null, (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig macro)
                {
                    _controller.OpenMacroLocation(macro);
                }
            });
            contextMenu.Items.Add(openLocationItem);

            ToolStripMenuItem propertiesItem = new ToolStripMenuItem("ℹ️ Properties", null, (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig macro)
                {
                    _controller.ShowMacroProperties(macro);
                }
            });
            contextMenu.Items.Add(propertiesItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem refreshItem = new ToolStripMenuItem("🔄 Refresh List", null, (s, e) =>
            {
                _controller.RefreshMacros();
            });
            contextMenu.Items.Add(refreshItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem deleteItem = new ToolStripMenuItem("🗑️ Delete", null, (s, e) =>
            {
                if (_macroTreeView.SelectedNode?.Tag is MacroConfig)
                {
                    _controller.DeleteCurrentMacro();
                }
            });
            deleteItem.BackColor = Color.FromArgb(244, 67, 54);
            deleteItem.ForeColor = Color.White;
            contextMenu.Items.Add(deleteItem);

            _macroTreeView.ContextMenuStrip = contextMenu;
            _macroTreeView.NodeMouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    _macroTreeView.SelectedNode = e.Node;
                }
            };
        }


        private void RefreshMacroTree()
        {
            if (_macroTreeView == null) return;

            _macroTreeView.Nodes.Clear();

            TreeNode rootNode = new TreeNode("Macros");
            _macroTreeView.Nodes.Add(rootNode);

            foreach (var macro in _model.LoadedMacros.OrderByDescending(m => m.LastUsed))
            {
                TreeNode macroNode = new TreeNode(macro.Name) { Tag = macro };
                rootNode.Nodes.Add(macroNode);
            }

            rootNode.Expand();
        }
    }
}


