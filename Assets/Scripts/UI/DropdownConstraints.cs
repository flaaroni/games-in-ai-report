using UnityEngine;

public class DropdownConstraints : MonoBehaviour
{
	[SerializeField]
	Constraints[] allOptions;

	[SerializeField]
	QuadGridCreator gridCreator;
	[SerializeField]
	TMPro.TMP_Dropdown dropdown;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		dropdown.ClearOptions();
		foreach (var constraint in allOptions)
		{
			dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(constraint.DisplayName));
		}
	}

	public void OnValueChanged(int value)
	{
		gridCreator.Constraints = allOptions[value];
	}
}
