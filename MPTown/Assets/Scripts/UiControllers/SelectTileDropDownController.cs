using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UiControllers
{
    [RequireComponent(typeof(Dropdown))]
    public class SelectTileDropDownController: MonoBehaviour
    {
        public void OnTileSelected(int optionIndex)
        {
            var selected = GetComponent<Dropdown>().options[optionIndex];
            var blockId = ushort.Parse(selected.text);
            FindObjectOfType<MapController>().EditBlockIndex = blockId;
        }
    }
}
