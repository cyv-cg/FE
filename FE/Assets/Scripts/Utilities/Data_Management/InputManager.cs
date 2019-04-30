using UnityEngine;

public static class InputManager
{
    private static readonly KeyCode leftClick = KeyCode.Mouse0;
    private static readonly KeyCode rightClick = KeyCode.Mouse1;

    public static bool LeftClick()
    {
        return Input.GetKey(leftClick);
    }
    public static bool LeftClickDown()
    {
        return Input.GetKeyDown(leftClick);
    }
    public static bool LeftClickUp()
    {
        return Input.GetKeyUp(leftClick);
    }

    public static bool RightClick()
    {
        return Input.GetKey(rightClick);
    }
    public static bool RightClickDown()
    {
        return Input.GetKeyDown(rightClick);
    }
    public static bool RightClickUp()
    {
        return Input.GetKeyUp(rightClick);
    }
}