using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    public Inventory _inventory;

    public ItemData[] _itemDataArray;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            for(int i =0;i<6;i++)
            _inventory.Add(_itemDataArray[i]);
            
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            for (int i = 6; i < 9; i++)
                _inventory.Add(_itemDataArray[i],100);

        }
        if (Input.GetKeyDown(KeyCode.J))
        {
        
            int capacity = _inventory.Capacity;
            for (int i = 0; i < capacity; i++)
                _inventory.Remove(i);
        };
    }
}
