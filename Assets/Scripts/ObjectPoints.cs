using UnityEngine;

public class ObjectPoints : MonoBehaviour
{
    [SerializeField] private int _points;
    [SerializeField] private string nameStick;
    [HideInInspector] public bool isFlagged = false;

    public int Points
    {
        get
        {
            return _points;
        }
        set
        {
            _points = value;
        }
    }

}   
