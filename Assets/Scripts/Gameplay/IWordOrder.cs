using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWordOrder
{
    public void OrderInPosition(bool isOverlapping, Transform overlappedWord);
}
