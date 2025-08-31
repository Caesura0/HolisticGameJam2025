using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(QuestObjective))]
public class QuestObjectiveDrawer : PropertyDrawer
{
    private string propertyPath = "";
    private SerializedProperty structure;
    private SerializedProperty objectiveType;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if(propertyPath != property.propertyPath)
            FindPropertyRelatives(property);
        
        EditorGUI.BeginProperty(position, label, property);
        

        Rect currentPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        if(structure == null ||  objectiveType == null)
        {
            EditorGUI.LabelField(currentPosition, "Error: property relative not found!");
            return;
        }

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(currentPosition, objectiveType);

        if(EditorGUI.EndChangeCheck())
            structure.managedReferenceValue = 
                QuestObjectiveStructure.GetStructureByType((QuestObjectiveType)objectiveType.enumValueIndex);
        
        MovePositionForward(ref currentPosition);

        if (structure.managedReferenceValue == null)
            EditorGUI.LabelField(currentPosition, "Error: no structure assigned to the objective type!");
        else
            DrawStructureProperties(ref currentPosition);

        EditorGUI.EndProperty();
    }

    private void FindPropertyRelatives(SerializedProperty property)
    {
        objectiveType = property.FindPropertyRelative(nameof(objectiveType));
        structure = property.FindPropertyRelative(nameof(structure));
        propertyPath = property.propertyPath;
    }

    private void DrawStructureProperties(ref Rect currentPosition)
    {
        SerializedProperty iterator = structure.Copy();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (SerializedProperty.EqualContents(iterator, structure.GetEndProperty()))
                break;

            MovePositionForward(ref currentPosition);
            EditorGUI.PropertyField(currentPosition, iterator, true);
        }
    }
    private void MovePositionForward(ref Rect currentPosition) =>
        currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty structureProp = property.FindPropertyRelative("structure");

        float height = EditorGUIUtility.singleLineHeight;

        if (structureProp != null && structureProp.managedReferenceValue != null)
        {
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;


            SerializedProperty iterator = structureProp.Copy();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (SerializedProperty.EqualContents(iterator, structureProp.GetEndProperty()))
                    break;

                height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
            }
        }
        else
        {
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
        return height;
    }
}