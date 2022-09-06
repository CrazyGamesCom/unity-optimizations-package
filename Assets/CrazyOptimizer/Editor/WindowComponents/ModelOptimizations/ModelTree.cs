using CrazyGames.TreeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CrazyGames.WindowComponents.ModelOptimizations
{
    class ModelTree : TreeViewWithTreeModel<ModelTreeItem>
    {
        public ModelTree(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader, TreeModel<ModelTreeItem> model)
            : base(treeViewState, multiColumnHeader, model)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;
            Reload();
        }

        void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
            {
                return; // No column to sort for (just use the order the data are in)
            }

            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            var items = rootItem.children.Cast<TreeViewItem<ModelTreeItem>>().OrderBy(i => i.data.modelName);
            var sortedColumnIndex = sortedColumns[0];
            var ascending = multiColumnHeader.IsSortedAscending(sortedColumnIndex);

            switch (sortedColumnIndex)
            {
                case 0:
                    items = items.Order(i => i.data.modelName, ascending);
                    break;
                case 1:
                    items = items.Order(i => i.data.isReadWriteEnabled, ascending);
                    break;
                case 2:
                    items = items.Order(i => i.data.arePolygonsOptimized, ascending);
                    break;
                case 3:
                    items = items.Order(i => i.data.areVerticesOptimized, ascending);
                    break;
                case 4:
                    items = items.Order(i => i.data.meshCompression, ascending);
                    break;
                case 5:
                    items = items.Order(i => i.data.animationCompression, ascending);
                    break;
            }

            rootItem.children = items.Cast<TreeViewItem>().ToList();
            TreeToList(root, rows);
            Repaint();
        }

        public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
        {
            if (root == null)
                throw new NullReferenceException("root");
            if (result == null)
                throw new NullReferenceException("result");

            result.Clear();

            if (root.children == null)
                return;

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();

            for (int i = root.children.Count - 1; i >= 0; i--)
                stack.Push(root.children[i]);

            while (stack.Count > 0)
            {
                TreeViewItem current = stack.Pop();
                result.Add(current);

                if (current.hasChildren && current.children[0] != null)
                {
                    for (int i = current.children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(current.children[i]);
                    }
                }
            }
        }

        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);
            SortIfNeeded(root, rows);
            return rows;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<ModelTreeItem>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
            }
        }

        private void CellGUI(Rect cellRect, TreeViewItem<ModelTreeItem> item, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);
            switch (column)
            {
                case 0:
                    GUI.Label(cellRect, item.data.modelName);
                    break;
                case 1:
                    GUI.Label(cellRect, item.data.isReadWriteEnabled ? "yes" : "no");
                    break;
                case 2:
                    GUI.Label(cellRect, item.data.arePolygonsOptimized ? "yes" : "no");
                    break;
                case 3:
                    GUI.Label(cellRect, item.data.areVerticesOptimized ? "yes" : "no");
                    break;
                case 4:
                    GUI.Label(cellRect, item.data.meshCompression);
                    break;
                case 5:
                    GUI.Label(cellRect, item.data.animationCompression);
                    break;
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            var item = treeModel.Find(selectedIds.First());
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(item.modelPath);
        }
    }
}