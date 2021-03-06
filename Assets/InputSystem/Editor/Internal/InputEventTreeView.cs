#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using ISX.LowLevel;

////TODO: add more information for each event (ideally, dump deltas that highlight control values that have changed)

////TODO: add diagnostics to immediately highlight problems with events (e.g. events getting ignored because of incorrect type codes)

namespace ISX.Editor
{
    // Multi-column TreeView that shows the events in a trace.
    internal class InputEventTreeView : TreeView
    {
        private InputEventPtr[] m_Events;
        private readonly InputEventTrace m_EventTrace;
        private readonly InputControl m_RootControl;

        private InputEventPtr GetEventPtrFromItemId(int id)
        {
            var eventIndex = id - 1;
            return m_Events[eventIndex];
        }

        private enum ColumnId
        {
            Id,
            Type,
            Device,
            Size,
            Time,
            COUNT
        }

        public static InputEventTreeView Create(InputDevice device, InputEventTrace eventTrace, ref TreeViewState treeState, ref MultiColumnHeaderState headerState)
        {
            if (treeState == null)
                treeState = new TreeViewState();

            var newHeaderState = CreateHeaderState();
            if (headerState != null)
                MultiColumnHeaderState.OverwriteSerializedFields(headerState, newHeaderState);
            headerState = newHeaderState;

            var header = new MultiColumnHeader(headerState);
            return new InputEventTreeView(treeState, header, eventTrace, device);
        }

        private static MultiColumnHeaderState CreateHeaderState()
        {
            var columns = new MultiColumnHeaderState.Column[(int)ColumnId.COUNT];

            columns[(int)ColumnId.Id] =
                new MultiColumnHeaderState.Column
            {
                width = 80,
                minWidth = 60,
                headerContent = new GUIContent("Id")
            };
            columns[(int)ColumnId.Type] =
                new MultiColumnHeaderState.Column
            {
                width = 60,
                minWidth = 60,
                headerContent = new GUIContent("Type")
            };
            columns[(int)ColumnId.Device] =
                new MultiColumnHeaderState.Column
            {
                width = 80,
                minWidth = 60,
                headerContent = new GUIContent("Device")
            };
            columns[(int)ColumnId.Size] =
                new MultiColumnHeaderState.Column
            {
                width = 50,
                minWidth = 50,
                headerContent = new GUIContent("Size")
            };
            columns[(int)ColumnId.Time] =
                new MultiColumnHeaderState.Column
            {
                width = 100,
                minWidth = 80,
                headerContent = new GUIContent("Time")
            };

            return new MultiColumnHeaderState(columns);
        }

        private InputEventTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, InputEventTrace eventTrace, InputControl rootControl)
            : base(state, multiColumnHeader)
        {
            m_EventTrace = eventTrace;
            m_RootControl = rootControl;
            Reload();
        }

        protected override void DoubleClickedItem(int id)
        {
            if (m_Events.Length == 0)
                return;

            var eventPtr = GetEventPtrFromItemId(id);

            // We can only inspect state events so ignore double-clicks on other
            // types of events.
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;

            var window = ScriptableObject.CreateInstance<InputStateWindow>();
            window.InitializeWithEvent(eventPtr, m_RootControl);
            window.Show();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem
            {
                id = 0,
                depth = -1,
                displayName = "Root"
            };

            ////FIXME: doing this over and over is very inefficient
            m_Events = m_EventTrace.ToArray();
            Array.Sort(m_Events,
                (a, b) =>
                {
                    var aTime = a.time;
                    var bTime = b.time;
                    if (aTime > bTime)
                        return -1;
                    if (bTime > aTime)
                        return 1;
                    return 0;
                });

            if (m_Events.Length == 0)
            {
                // TreeView doesn't allow having empty trees. Put a dummy item in here that we
                // render without contents.
                root.AddChild(new TreeViewItem(1));
            }

            for (var i = 0; i < m_Events.Length; ++i)
            {
                var eventPtr = m_Events[i];

                var item = new TreeViewItem
                {
                    id = i + 1,
                    depth = 1,
                    displayName = eventPtr.id.ToString()
                };

                root.AddChild(item);
            }

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            // Render nothing if event list is empty.
            if (m_Events.Length == 0 || args.item.id <= 0 || args.item.id > m_Events.Length)
                return;

            var eventPtr = GetEventPtrFromItemId(args.item.id);

            var columnCount = args.GetNumVisibleColumns();
            for (var i = 0; i < columnCount; ++i)
            {
                ColumnGUI(args.GetCellRect(i), eventPtr, args.GetColumn(i), ref args);
            }
        }

        private void ColumnGUI(Rect cellRect, InputEventPtr eventPtr, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case (int)ColumnId.Id:
                    GUI.Label(cellRect, eventPtr.id.ToString());
                    break;
                case (int)ColumnId.Type:
                    GUI.Label(cellRect, eventPtr.type.ToString());
                    break;
                case (int)ColumnId.Device:
                    GUI.Label(cellRect, eventPtr.deviceId.ToString());
                    break;
                case (int)ColumnId.Size:
                    GUI.Label(cellRect, eventPtr.sizeInBytes.ToString());
                    break;
                case (int)ColumnId.Time:
                    GUI.Label(cellRect, eventPtr.time.ToString("0.0000s"));
                    break;
            }
        }
    }
}
#endif // UNITY_EDITOR
