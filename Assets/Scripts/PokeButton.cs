using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PokeButton : MonoBehaviour
{
    [SerializeField] private Mesh offMesh;
    [SerializeField] private Mesh onMesh;
    private MeshFilter _meshFilter;
    private bool _isCurrentlyActive;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    public void ChangeMesh()
    {
        var mesh = _isCurrentlyActive ? offMesh : onMesh;
        _meshFilter.mesh = mesh;
        _isCurrentlyActive = !_isCurrentlyActive;
    }
}