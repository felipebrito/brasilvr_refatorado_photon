using UnityEngine;
using UnityEngine.UI;

public class SlotSelector : MonoBehaviour
{
    [SerializeField] private GameObject slotSelectionPanel;
    [SerializeField] private Button[] slotButtons;
    [SerializeField] private MonoBehaviour photonController;

    private void Start()
    {
        if (photonController != null)
            photonController.enabled = false;

        if (PlayerPrefs.HasKey("VRSlot"))
        {
            if (photonController != null)
                photonController.enabled = true;
            if (slotSelectionPanel != null)
                Destroy(slotSelectionPanel);
            return;
        }

        if (slotSelectionPanel != null)
            slotSelectionPanel.SetActive(true);

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slot = i;
            slotButtons[i].onClick.RemoveAllListeners();
            slotButtons[i].onClick.AddListener(() => SelectSlot(slot));
        }
    }

    private void SelectSlot(int slotIndex)
    {
        PlayerPrefs.SetInt("VRSlot", slotIndex);
        PlayerPrefs.Save();

        if (photonController != null)
            photonController.enabled = true;

        if (slotSelectionPanel != null)
            Destroy(slotSelectionPanel);
    }
}
