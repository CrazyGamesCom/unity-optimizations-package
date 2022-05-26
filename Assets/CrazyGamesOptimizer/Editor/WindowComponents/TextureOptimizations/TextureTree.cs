using System;
using System.Collections.Generic;
using System.Linq;
using CrazyGames.TreeLib;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CrazyGames.WindowComponents.TextureOptimizations
{
    class MultiColumnTree : TreeViewWithTreeModel<TextureTreeItem>
    {
        public MultiColumnTree(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader, TreeModel<TextureTreeItem> model)
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

            var items = rootItem.children.Cast<TreeViewItem<TextureTreeItem>>().OrderBy(i => i.data.textureName);
            var sortedColumnIndex = sortedColumns[0];
            var ascending = multiColumnHeader.IsSortedAscending(sortedColumnIndex);
            switch (sortedColumnIndex)
            {
                case 0:
                    items = items.Order(i => i.data.textureName, ascending);
                    break;
                case 1:
                    items = items.Order(i => i.data.textureType, ascending);
                    break;
                case 2:
                    items = items.Order(i => i.data.textureMaxSize, ascending);
                    break;
                case 3:
                    items = items.Order(i => i.data.compression, ascending);
                    break;
                case 4:
                    items = items.Order(i => i.data.crunchCompressionQuality, ascending);
                    break;
                case 5:
                    items = items.Order(i => i.data.crunchCompressionQuality, ascending);
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
            var item = (TreeViewItem<TextureTreeItem>) args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
            }
        }

        private void CellGUI(Rect cellRect, TreeViewItem<TextureTreeItem> item, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);
            switch (column)
            {
                case 0:
                    GUI.Label(cellRect, item.data.textureName);
                    break;
                case 1:
                    GUI.Label(cellRect, item.data.textureType.ToString());
                    break;
                case 2:
                    GUI.Label(cellRect, item.data.textureMaxSize.ToString());
                    break;
                case 3:
                    GUI.Label(cellRect, item.data.compression.ToString());
                    break;
                case 4:
                    GUI.Label(cellRect, item.data.hasCrunchCompression ? "yes" : "no");
                    break;
                case 5:
                    GUI.Label(cellRect, item.data.crunchCompressionQuality.ToString());
                    break;
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            var item = treeModel.Find(selectedIds.First());
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(item.texturePath);
        }
    }
}