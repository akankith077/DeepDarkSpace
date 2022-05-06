using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class LoggingScript : MonoBehaviour
{
    // Start is called before the first frame update
    public CarpetNav carNavScript;

    public List<Vector3> carPositions = new List<Vector3>();
    public List<Vector3> backToCarpetPostions = new List<Vector3>();
    public List<string> timeSting = new List<string>();
    public List<string> jumpTypeList = new List<string>();
    public int jumpCount;
    public int presenterJump = 0;
    public int circleJump = 0;
    public int SemiCircleJump = 0;
    public int NormalJump = 0;
    public int backToCarpetJumps = 0;
    private int index=1;
    private string jumpType;
    string strSeperator = ",";

    public void backToCarpetData(Vector3 postions)
    {
        if (!backToCarpetPostions.Contains(postions))
        {
            backToCarpetJumps += 1;
            backToCarpetPostions.Add(postions);
        }
    }
    public void addData(int type, Vector3 carPos)
    {
        if (!carPositions.Contains(carPos))
        {
            jumpType = null;
            if(type == 1)
            {
                circleJump += 1;
                jumpType = "Circle Formation";
            }
            if (type == 2)
            {
                SemiCircleJump += 1;
                jumpType = "SemiCircle Formation";
            }
            if (type == 3)
            {
                presenterJump += 1;
                jumpType = "Presenter Formation";
            }
            if (type == 4)
            {
                NormalJump += 1;
                jumpType = "Normal Formation";
            }
            jumpTypeList.Add(jumpType);
            carPositions.Add(carPos);
            timeSting.Add(DateTime.Now.ToString("h:mm:ss tt")); 
        }
    }
    public void printData()
    { 
        StringBuilder sbOutput = new StringBuilder();
        string strFilePath = @"C:\Users\akank\ThesisData\MTC.csv";
        List<string> loggingBody = new List<string>();
        loggingBody.Add("Index,Time(s),Type of jump,Jump Location(s)");
        string JumpIime, TypeOfJump, JumpLocations;
        float totalJumps = circleJump + SemiCircleJump + presenterJump + NormalJump;



        for(index = 0; index < carPositions.Count; index++)
        {
            JumpIime = timeSting[index];
            TypeOfJump = jumpTypeList[index];
            JumpLocations = carPositions[index].ToString("F3");
            loggingBody.Add((index + 1) + strSeperator + JumpIime + strSeperator + TypeOfJump + strSeperator + JumpLocations );
        }

            loggingBody.Add("Index, BTC Jumps");
        for(int i = 0; i < backToCarpetPostions.Count; i++)
        {
            JumpLocations = backToCarpetPostions[i].ToString("F3");
            loggingBody.Add((index + 1) +  strSeperator + JumpLocations);
        }

        loggingBody.Add("Cicle formation, SemiCircle Formation, Presenter Formations, As-is Formation, Total Jumps");
        loggingBody.Add(circleJump + strSeperator + SemiCircleJump + strSeperator + presenterJump + strSeperator + NormalJump + strSeperator + totalJumps);

        for (int i = 0; i < loggingBody.Count; i++)
        {
            sbOutput.AppendLine(loggingBody[i]);
        }

        File.WriteAllText(strFilePath, sbOutput.ToString());
    }
}
