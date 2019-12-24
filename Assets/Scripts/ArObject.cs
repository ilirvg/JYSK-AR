using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArObject : MonoBehaviour {

    private List<GameObject> models;

    private int selectionIndex = 0;

    private void Start() {
        models = new List<GameObject>();
        foreach (Transform t in transform) {
            models.Add(t.gameObject);
            t.gameObject.SetActive(false);
        }
        models[selectionIndex].SetActive(true);
    }

    public void Select(int index) {
        if (index == selectionIndex) {
            return;
        }
        if (index < 0 || index >= models.Count) {
            return;
        }
        models[selectionIndex].SetActive(false);
        models[selectionIndex].GetComponent<MeshRenderer>().enabled = true;
        models[selectionIndex].GetComponent<BoxCollider>().enabled = true;
        selectionIndex = index;
        models[selectionIndex].SetActive(true);
        models[selectionIndex].GetComponent<MeshRenderer>().enabled = true;
        models[selectionIndex].GetComponent<BoxCollider>().enabled = true;


    }
}
