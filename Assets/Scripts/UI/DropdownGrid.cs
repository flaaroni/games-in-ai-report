using UnityEngine;

public class DropdownGrid : MonoBehaviour
{
	[SerializeField]
	IGridMeshes[] allOptions;

	[SerializeField]
	QuadGridCreator gridCreator;
	[SerializeField]
	TMPro.TMP_Dropdown dropdown;
	[SerializeField]
	Transform cameraTransform;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		dropdown.ClearOptions();
		foreach (var mesh in allOptions)
		{
			dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(mesh.GridName));
		}
		dropdown.value = 0;
		OnValueChanged(0);
	}

	public void OnValueChanged(int value)
	{
		gridCreator.Grid = allOptions[value];
		cameraTransform.position = allOptions[value].CameraPosition;
	}
}
