using UnityEditor;
using UnityEngine;

// Code from https://devsourcehub.com/unitys-property-drawers-customizing-inspector-appearance/
[CustomPropertyDrawer(typeof(Constraints.MaterialPair))]
public class MaterialPairDrawer : PropertyDrawer
{
	const float GAP = 5;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		// Start the property drawing process.
		EditorGUI.BeginProperty(position, label, property);

		// Calculate the width of each field, accounting for the gap between them.
		float width = (position.width - GAP) / 2f;

		// Calculate rects for left half
		var rect = new Rect(position.x, position.y, width, position.height);
		SerializedProperty prop = property.FindPropertyRelative("left");
		EditorGUI.PropertyField(rect, prop, GUIContent.none);

		// Calculate rects for right half
		rect.x += width + GAP;
		prop = property.FindPropertyRelative("right");
		EditorGUI.PropertyField(rect, prop, GUIContent.none);
		EditorGUI.EndProperty();
	}
}
