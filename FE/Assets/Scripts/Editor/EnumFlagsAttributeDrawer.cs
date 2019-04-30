using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumFlagAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer
{
    EnumFlagAttribute Flag { get { return (EnumFlagAttribute)attribute; } }

    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        int maxPerRow = Flag.max;
        float padding = 1.1f;

        int buttonsIntValue = 0;
        int enumLength = _property.enumNames.Length;
        
        bool[] buttonPressed = new bool[enumLength];
        float buttonWidth = (_position.width - EditorGUIUtility.labelWidth) / maxPerRow;

        EditorGUI.LabelField(new Rect(_position.x, _position.y, EditorGUIUtility.labelWidth, _position.height), _label);

        EditorGUI.BeginChangeCheck();

        int row = 0;

        for (int i = 0; i < enumLength; i++)
        {
            if (i == (row + 1) * maxPerRow)
                row++;

            // Check if the button is/was pressed 
            if ((_property.intValue & (1 << i)) == 1 << i)
            {
                buttonPressed[i] = true;
            }

            float x = _position.x + EditorGUIUtility.labelWidth + buttonWidth * i;

            Rect buttonPos = new Rect(x - buttonWidth * maxPerRow * row, _position.y + _position.height * Mathf.FloorToInt(i / maxPerRow) * padding, buttonWidth, _position.height * padding);

            buttonPressed[i] = GUI.Toggle(buttonPos, buttonPressed[i], _property.enumNames[i], "Button");

            if (buttonPressed[i])
                buttonsIntValue += 1 << i;
        }

        if (EditorGUI.EndChangeCheck())
        {
            _property.intValue = buttonsIntValue;
        }

        GUILayout.Space(3 + 16 * row * padding);
    }
}