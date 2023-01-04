using System.Collections;
using UnityEngine;
using UnityOSC;
using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine.Events;

public class joyconharvester : MonoBehaviour
{
    private int rumble = 0;
    private int rumbleLen = 100;
    private int rumbleFreqLow = 160;
    private int rumbleFreqHigh = 320;
    private OSCServer myServer;


    public string outIP = "127.0.0.1";

    private List<Joycon> joycons;

    public int outPort = 7000;
    public int inPort = 7001;

    // Values made available via Unity
    private Vector2 stick;
    private Vector3 gyro;
    private Vector3 accel;
    int jc_ind = 0;
    private Quaternion orientation;
    // Use this for initialization

    void Start()
    {

        // init OSC
        OSCHandler.Instance.Init();

        // Initialize OSC clients (transmitters)
        OSCHandler.Instance.CreateClient("myClient", IPAddress.Parse(outIP), outPort);

        // Initialize OSC servers (listeners)
        myServer = OSCHandler.Instance.CreateServer("myServer", inPort);
        // Set buffer size (bytes) of the server (default 1024)
        myServer.ReceiveBufferSize = 1024;
        // Set the sleeping time of the thread (default 10)
        myServer.SleepMilliseconds = 10;

        stick = new Vector2(0, 0);
        gyro = new Vector3(0, 0, 0);
        accel = new Vector3(0, 0, 0);
        // get the public Joycon array attached to the JoyconManager in scene
        joycons = JoyconManager.Instance.j;
        if (joycons.Count < jc_ind + 1)
        {
            //Destroy(gameObject);
        }
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        int jcount = joycons.Count;
        if (OSCHandler.Instance.Clients.Count < 1) return;

        for (int jc = 0; jc < jcount; jc++) {
            Joycon j = joycons[jc];
            string jroot = "/" + jc;
            stick = new Vector2(j.GetStick()[0], j.GetStick()[1]);
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/joystick/x", stick.x);
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/joystick/y", stick.y);

            // Gyro values: x, y, z axis values (in radians per second)
            gyro = j.GetGyro();
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/gyro/x", gyro.x);
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/gyro/y", gyro.y);
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/gyro/z", gyro.z);
            // Accel values:  x, y, z axis values (in Gs)
            accel = j.GetAccel();
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/accel/x", accel.x);
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/accel/y", accel.y);
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/accel/z", accel.z);

            orientation = j.GetVector();
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/orientation/0", orientation[0]);
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/orientation/1", orientation[1]);
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/orientation/2", orientation[2]);
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/orientation/3", orientation[3]);

            //Buttons
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/buttons/down", (j.GetButton(Joycon.Button.DPAD_DOWN) ? 1 : 0));
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/buttons/up", (j.GetButton(Joycon.Button.DPAD_UP) ? 1 : 0));
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/buttons/right", (j.GetButton(Joycon.Button.DPAD_RIGHT) ? 1 : 0));
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/buttons/left", (j.GetButton(Joycon.Button.DPAD_LEFT) ? 1 : 0));
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/buttons/minus", (j.GetButton(Joycon.Button.MINUS) ? 1 : 0));
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/buttons/bumper", (j.GetButton(Joycon.Button.SHOULDER_1) ? 1 : 0));
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/buttons/trigger", (j.GetButton(Joycon.Button.SHOULDER_2) ? 1 : 0));
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/buttons/home", (j.GetButton(Joycon.Button.CAPTURE) ? 1 : 0));
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/buttons/joystick", (j.GetButton(Joycon.Button.STICK) ? 1 : 0));
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/buttons/side-top", (j.GetButton(Joycon.Button.SL) ? 1 : 0));
            OSCHandler.Instance.SendMessageToClient("myClient", jroot+"/buttons/side-bottom", (j.GetButton(Joycon.Button.SR) ? 1 : 0));

            if (rumble == 1)
            {
                j.SetRumble(rumbleFreqLow, rumbleFreqHigh, 0.6f, rumbleLen);
                rumble = 0;
            }
        


        for (var i = 0; i < OSCHandler.Instance.packets.Count; i++)
        {
            // Process OSC
            receivedOSC(OSCHandler.Instance.packets[i]);
            // Remove them once they have been read.
            OSCHandler.Instance.packets.Remove(OSCHandler.Instance.packets[i]);
            i--;
        }
    }
        }
    public void changePort(string p)
    {
        int port;
        bool success = int.TryParse(p, out port);
        if (!success)
        {
            print("failed to change port");
            return;
        }
        outPort = port;
        OSCHandler.Instance.Clients.Clear();
        OSCHandler.Instance.CreateClient("myClient", IPAddress.Parse(outIP), outPort);
        print("port changed to " + port);

    }


    private void receivedOSC(OSCPacket pckt)
    {
        if (pckt == null) { return; }

        int serverPort = pckt.server.ServerPort;

        // Address
        string address = pckt.Address.Substring(1);
       
        // Data at index 0
        string data0 = pckt.Data.Count != 0 ? pckt.Data[0].ToString() : "null";
        Debug.Log(address + " " + data0);

        if (address == "rumble")
        {
            rumble = 1;
        }

        if (address == "rumble/length")
        {
            rumbleLen = System.Convert.ToInt32(data0);
        }

        if (address == "rumble/freqLow")
        {
            rumbleFreqLow = System.Convert.ToInt32(data0);
        }

        if (address == "rumble/freqHigh")
        {
            rumbleFreqHigh = System.Convert.ToInt32(data0);
        }

    }


}

