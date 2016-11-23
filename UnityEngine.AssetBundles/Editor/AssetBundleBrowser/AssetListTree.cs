﻿using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System;


namespace UnityEngine.AssetBundles
{
	internal class AssetListTree : TreeView
	{
        IEnumerable<AssetBundleState.BundleInfo> m_selectedBundles;
        HashSet<AssetBundleState.AssetInfo> m_assetsInSelectedBundles = new HashSet<AssetBundleState.AssetInfo>();
        SelectionListTree m_selectionList;

		public AssetListTree(TreeViewState state, SelectionListTree selList) : base(state)
		{
            m_selectionList = selList;
            Reload();
		}

        protected override void BuildRootAndRows(out TreeViewItem root, out IList<TreeViewItem> rows)
		{
			root = new TreeViewItem(-1, -1);
			rows = new List<TreeViewItem>();
            rows.Add(root);
           // for (int i = 0; i < m_assetsInSelectedBundles.Count; i++)
           foreach(var a in m_assetsInSelectedBundles)
            {
                //var assetIndex = m_assetsInSelectedBundles[i];
                //var assetName = AssetBundleState.assets[assetIndex].name;
                var item = new TreeViewItem(a.name.GetHashCode(), 0, root, System.IO.Path.GetFileNameWithoutExtension(a.name));
                item.userData = a;
                item.icon = AssetDatabase.GetCachedIcon(a.name) as Texture2D;
                rows.Add(item);
                root.AddChild(item);
            }

            //  foreach (var b in m_assetsInSelectedBundles)
            //     CreateItems(rows, b, 0);

            //SetupParentsAndChildrenFromDepths(root, rows);
        }
        /*
        void CreateItems(IList<TreeViewItem> rows, AssetBundleState.AssetData a, int depth)
        {
            Item item = new Item(a, depth);
            rows.Add(item);
            var dependencies = AssetDatabase.GetDependencies(a.m_assetPath, false);
            if (IsExpanded(a.m_id))
            {
                foreach (var d in dependencies)
                {
                    if (d != a.m_assetPath)
                    {
                        AssetBundleState.AssetData ad = AssetBundleState.GetAssetData(string.Empty, d);
                        if (string.IsNullOrEmpty(ad.m_bundle))
                            CreateItems(rows, ad, depth + 1);
                    }
                }
            }
            else
            {
                if(dependencies.Length > 0 && dependencies[0] != a.m_assetPath)
                    item.children = CreateChildListForCollapsedParent();
            }
        }
        */


        internal void SetSelectedBundles(IEnumerable<AssetBundleState.BundleInfo> b)
        {
            m_selectedBundles = b;
            m_assetsInSelectedBundles.Clear();
            if (HasSelection())
                SetSelection(new List<int>());

            foreach (var bundleInfo in m_selectedBundles)
                foreach (var a in bundleInfo.assets)
                    m_assetsInSelectedBundles.Add(a);
            SelectionChanged(GetSelection());
            Reload();
        }

        protected override void ContextClickedItem(int id)
        {
            var i = TreeViewUtility.FindItem(id, rootItem);
            if (i != null)
            {
                GenericMenu menu = new GenericMenu();
                foreach(var b in AssetBundleState.bundles)
                    if(!m_selectedBundles.Contains(b.Value))
                        menu.AddItem(new GUIContent("Move to bundle/" + b.Key), false, MoveToBundle, b.Value);
                menu.AddItem(new GUIContent("Move to bundle/<Create New Bundle...>"), false, MoveToBundle, null);
                menu.ShowAsContext();
            }
        }

        void MoveToBundle(object target)
        {
            AssetBundleState.BundleInfo bi = target as AssetBundleState.BundleInfo;
            if (bi == null)
            {
                if (EditorUtility.DisplayDialogComplex("Create Bundle Name", "Hit ok to create new bundle", "Create", "Cancel", "idk?") > 0)
                    return;
                bi = AssetBundleState.CreateEmptyBundle("New Bundle" + Random.Range(0, 10000));
            }

            AssetBundleState.MoveAssetsToBundle(bi, GetRowsFromIDs(GetSelection()).Select(a => a.userData as AssetBundleState.AssetInfo));
            SetSelectedBundles(m_selectedBundles);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
		{
            m_selectionList.SetItems(GetRowsFromIDs(GetSelection()).Select(a => a.userData as AssetBundleState.AssetInfo));
		}

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            args.draggedItemIDs = GetSelection();
            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.paths = GetRowsFromIDs(args.draggedItemIDs).Select(a => (a.userData as AssetBundleState.AssetInfo).name).ToArray();
            DragAndDrop.StartDrag("blah");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            return DragAndDropVisualMode.None;
        }
    }


}