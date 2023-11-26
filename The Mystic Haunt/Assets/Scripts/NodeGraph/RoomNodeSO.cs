using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code

#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSlected = false;
    
    public void Initialise(Rect rect,RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public void Draw(GUIStyle nodeStyle)
    {
        GUILayout.BeginArea(rect,nodeStyle);
        
        EditorGUI.BeginChangeCheck();

        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor
                || !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor
                || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = childRoomNodeIDList.Count -1; i>=0 ; i--)
                    {
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);
                        if (childRoomNode != null && childRoomNode.isSlected)
                        {
                            RemoveChildRoomNodeIdFromRoomNode(childRoomNode.id);

                            childRoomNode.RemoveParentRoomNodeIdFromRoomNode(id);
                        }
                    }
                }
            }
        }

        if(EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);
        
        GUILayout.EndArea();
    }

    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];
        for (int i = 0; i < roomArray.Length; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
        }
        return roomArray;
    }

    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown: ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp: ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag: ProcessMouseDragEvent(currentEvent);
                break;
        }
    }

    public bool AddChildRoomNodeIDTToRoomNode(string childID)
    {
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }
        return false;
    }

    bool IsChildRoomValid(string childId)
    {
        bool isConnectedBossNodeAlready = false;
        foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossNodeAlready = true;
            }
        }
        
        if(roomNodeGraph.GetRoomNode(childId).roomNodeType.isBossRoom&&isConnectedBossNodeAlready)
            return false;
        
        if(roomNodeGraph.GetRoomNode(childId).roomNodeType.isNone)
            return false;
        
        if(childRoomNodeIDList.Contains(childId))
            return false;

        if (id == childId)
            return false;

        if (parentRoomNodeIDList.Contains(childId))
            return false;
        
        if(roomNodeGraph.GetRoomNode(childId).parentRoomNodeIDList.Count>0)
            return false;
        
        if(roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor&&roomNodeType.isCorridor)
            return false;
        
        if(!roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor&&!roomNodeType.isCorridor)
            return false;
        
        if(roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor&& childRoomNodeIDList.Count>=Settings.maxChildCorridors)
            return false;
        
        if(roomNodeGraph.GetRoomNode(childId).roomNodeType.isEntrance)
            return false;
        
        if(!roomNodeGraph.GetRoomNode(childId).roomNodeType.isCorridor&&childRoomNodeIDList.Count>0)
            return false;

        return true;
    }

    public bool AddParentRoomNodeIDTToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    public bool RemoveChildRoomNodeIdFromRoomNode(string childId)
    {
        if (childRoomNodeIDList.Contains(childId))
        {
            childRoomNodeIDList.Remove(childId);
            return true;
        }
        return false;
    }
    
    public bool RemoveParentRoomNodeIdFromRoomNode(string parentId)
    {
        if (parentRoomNodeIDList.Contains(parentId))
        {
            parentRoomNodeIDList.Remove(parentId);
            return true;
        }
        return false;
    }

    void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }
    
    void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }
    
    void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }
    
    

    void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;
        isSlected = !isSlected;
    }
    void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }
    void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }
    void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this,currentEvent.mousePosition);
    }
#endif

    #endregion
}
