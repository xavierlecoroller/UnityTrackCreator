using UnityEngine;
using System.Collections;

public class TronconConfig{
    public TypeOfTroncon TypeOfTheTroncon;
    public int BeginNum;
    public int EndNum;

    public TronconConfig(TypeOfTroncon typeOfTheTroncon, int beginNum, int endNum){
        TypeOfTheTroncon = typeOfTheTroncon;
        BeginNum = beginNum;
        EndNum = endNum;
    }
}