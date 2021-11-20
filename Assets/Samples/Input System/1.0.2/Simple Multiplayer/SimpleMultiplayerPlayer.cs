using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleMultiplayerPlayer : MonoBehaviour
{
    private InputAction pAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>:P");
    private void Awake()
    {
        pAction.performed += context => Debug.LogError("P pressed");
    }
    // Just add *some* kind of movement. The specifics here are not of interest. Serves just to
    // demonstrate that the inputs are indeed separate.
    public void OnTeleport()
    {
        //transform.position = new Vector3(Random.Range(-75, 75), 0.5f, Random.Range(-75, 75));
    }

    public void OnMove(InputValue value)
    {
        var v = value.Get<Vector2>();
        transform.position += Vector3.forward * v.y;
        transform.position += Vector3.right * v.x;
    }
}
