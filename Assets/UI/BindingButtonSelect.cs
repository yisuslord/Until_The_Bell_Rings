using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class BindingButtonSelect : MonoBehaviour
{
    [SerializeField] private Button toSelectButton;

    private void OnEnable()
    {
        StartCoroutine(SelectButton(toSelectButton));
    }

    private IEnumerator SelectButton(Button toSelect)
    {
        yield return null;

        EventSystem.current.SetSelectedGameObject(toSelect.gameObject);
    }
}
