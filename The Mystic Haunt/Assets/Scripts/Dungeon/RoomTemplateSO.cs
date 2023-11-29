using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_",menuName = "Scriptable Objects/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    [HideInInspector] public string guid;

    #region Header ROOM PREFAB

    [Space(10)]
    [Header("ROOM PREFAB")]

    #endregion Header ROOM PREFAB
    #region Tooltip

    [Tooltip(
        "The gameobject prefab for the room (this will contain all the tilemaps for the room and environment game objects)")]

    #endregion Tooltip
    public GameObject prefab;

    [HideInInspector]
    public GameObject previousPrefab;
    
    #region Header ROOM CONFIGURATION

    [Space(10)] [Header("ROOM CONFIGURATION")]

    #endregion Header ROOM CONFIGURATION

    public RoomNodeTypeSO roomNodeType;

    public Vector2Int lowerBounds;
    
    public Vector2Int upperBounds;

    [SerializeField]
    public List<Doorway> doorwayList;

    public Vector2Int[] spawnPositionArray;

    public List<Doorway> GetDoorwayList() => doorwayList;

    #region Validation

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }
        
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPositionArray), spawnPositionArray);
    }
#endif
    #endregion
}
