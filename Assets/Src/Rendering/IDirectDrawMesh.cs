using System;
using UnityEngine;


public interface IDirectDrawMesh {

    event Action<UIElement, Mesh> onMeshUpdate;

}