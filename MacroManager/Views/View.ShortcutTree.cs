using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MacroManager.Models;

namespace MacroManager
{
    public partial class View
    {
        private void CreateShortcutTree(Control parent)
        {
            _shortcutTreeView = new TreeView
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

            CreateShortcutTreeViewContextMenu();
            RefreshShortcutTree();

            _shortcutTreeView.NodeMouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && e.Node.Tag is MacroConfig shortcut)
                {
                    _controller.LoadShortcut(shortcut);
                }
            };

            _shortcutTreeView.DoubleClick += (s, e) =>
            {
                if (_shortcutTreeView.SelectedNode?.Tag is MacroConfig)
                {
                    _controller.RenameCurrentShortcut();
                }
            };

            _shortcutTreeView.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete && _shortcutTreeView.SelectedNode?.Tag is MacroConfig)
                {
                    _controller.DeleteCurrentShortcut();
                }
            };

            parent.Controls.Add(_shortcutTreeView);
        }

        private void CreateShortcutTreeViewContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip
            {
                BackColor = _model.CardBackColor,
                ForeColor = _model.PanelForeColor,
                Font = _model.CreateFont(9)
            };

            ToolStripMenuItem renameItem = new ToolStripMenuItem("✏️ Rename", null, (s, e) =>
            {
                if (_shortcutTreeView.SelectedNode?.Tag is MacroConfig)
                {
                    _controller.RenameCurrentShortcut();
                }
            });
            contextMenu.Items.Add(renameItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem duplicateItem = new ToolStripMenuItem("📋 Duplicate", null, (s, e) =>
            {
                if (_shortcutTreeView.SelectedNode?.Tag is MacroConfig)
                {
                    _controller.DuplicateCurrentShortcut();
                }
            });
            contextMenu.Items.Add(duplicateItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem openLocationItem = new ToolStripMenuItem("📁 Open Location", null, (s, e) =>
            {
                if (_shortcutTreeView.SelectedNode?.Tag is MacroConfig shortcut)
                {
                    _controller.OpenShortcutLocation(shortcut);
                }
            });
            contextMenu.Items.Add(openLocationItem);

            ToolStripMenuItem propertiesItem = new ToolStripMenuItem("ℹ️ Properties", null, (s, e) =>
            {
                if (_shortcutTreeView.SelectedNode?.Tag is MacroConfig shortcut)
                {
                    _controller.ShowShortcutProperties(shortcut);
                }
            });
            contextMenu.Items.Add(propertiesItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem refreshItem = new ToolStripMenuItem("🔄 Refresh List", null, (s, e) =>
            {
                _controller.RefreshShortcuts();
            });
            contextMenu.Items.Add(refreshItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem deleteItem = new ToolStripMenuItem("🗑️ Delete", null, (s, e) =>
            {
                if (_shortcutTreeView.SelectedNode?.Tag is MacroConfig)
                {
                    _controller.DeleteCurrentShortcut();
                }
            });
            deleteItem.BackColor = Color.FromArgb(244, 67, 54);
            deleteItem.ForeColor = Color.White;
            contextMenu.Items.Add(deleteItem);

            _shortcutTreeView.ContextMenuStrip = contextMenu;
            _shortcutTreeView.NodeMouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    _shortcutTreeView.SelectedNode = e.Node;
                }
            };
        }

        private void RefreshShortcutTree()
        {
            if (_shortcutTreeView == null) return;

            _shortcutTreeView.Nodes.Clear();

            TreeNode rootNode = new TreeNode("Shortcuts");
            _shortcutTreeView.Nodes.Add(rootNode);

            foreach (var shortcut in _model.LoadedShortcuts.OrderByDescending(s => s.LastUsed))
            {
                TreeNode shortcutNode = new TreeNode(shortcut.Name) { Tag = shortcut };
                rootNode.Nodes.Add(shortcutNode);
            }

            rootNode.Expand();
        }
    }
}

