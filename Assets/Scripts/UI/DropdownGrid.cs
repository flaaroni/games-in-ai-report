using UnityEngine;
using UnityEngine.UI;

public class DropdownGrid : MonoBehaviour
{
	[SerializeField]
	IGridMeshes[] allOptions;

	[SerializeField]
	QuadGridCreator gridCreator;
	[SerializeField]
	TMPro.TMP_Dropdown dropdown;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		dropdown.ClearOptions();
		foreach (var mesh in allOptions)
		{
			dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(mesh.name));
		}
    }

	public void OnValueChanged(int value)
	{
		gridCreator.Grid = allOptions[value];
	}
}
