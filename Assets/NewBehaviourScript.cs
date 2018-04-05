
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[Serializable] //makes it possible to go from JSON to object, variables below must match what is in JSON data
public class TelemetryData
{
    public float p_sub;       // SUB PRESSURE [psia] - External Environment pressure - Expected range is from 2 to 4 psia
    public float t_sub;       // SUB TEMPERATURE [degrees Fahrenheit] - External Environmental temperature measured in degrees Fahrenheit
    public int v_fan;         // FAN TACHOMETER [RPM] - Speed of the cooling fan - Expected range is from 10000 to 40000 RPM
    public string t_eva;      // EXTRAVEHICULAR ACTIVITY TIME [time value] - Stopwatch for the current EVA. Important indicator for astronauts to monitor current process of the task on-hand.
    public int p_o2;          // OXYGEN PRESSURE [psia] - Pressure inside the Primary Oxygen Pack - Expected range is from 750 to 950 psia.
    public float rate_o2;     // OXYGEN RATE [psi/min] - Flowrate of the Primary Oxygen Pack - Expected range is from 0.5 to 1 psi/min
    public float cap_battery; // BATTERY CAPACITY [amp-hr] - Total capacity of the spacesuit’s battery - Expected range is from 0 to 30 amp-hr
    public float p_h2o_g;     // H2O GAS PRESSURE [psia] - Gas pressure from H2O system - Expected range is from 14 to 16 psia
    public float p_h2o_l;     // H2O LIQUID PRESSURE [psia] - Liquid pressure from H2O system - Expected range is from 14 to 16 psia
    public float p_sop;       // SOP PRESSURE [psia] - Pressure inside the Secondary Oxygen Pack - Expected range is from 750 to 950 psia.
    public float rate_sop;    // SOP RATE [psi/min] - Flowrate of the Secondary Oxygen Pack - Expected range is from 0.5 to 1 psi/min.
}

/*
public class DisplayedTelemetryFields
{
    public Boolean p_sub;
    public Boolean t_sub;
    public Boolean v_fan;
    public Boolean t_eva;
    public Boolean p_o2;
    public Boolean rate_o2;
    public Boolean cap_battery;
    public Boolean p_h2o_g;
    public Boolean p_h2o_l;
    public Boolean p_sop;
    public Boolean rate_sop;
}
*/

[Serializable] //same thing as before when you get data, serialize into this class (as if its correctly doing T/F) ---fix to be correct in app!!!!
public class SwitchData
{
    public Boolean sop_on;
    public Boolean sspe;
    public Boolean fan_error;
    public Boolean vent_error;
    public Boolean vehicle_power;
    public Boolean h2o_off;
    public Boolean o2_off;
}

public class NewBehaviourScript : MonoBehaviour {

    Text telemetryBox; 
    Text alertsBox;

    TelemetryData telemetrydata = new TelemetryData();
    SwitchData switchdata = new SwitchData();

    static string telemetryServer = "http://127.0.0.1";

    string telemetryURL = telemetryServer + "/api/telemetry/recent";
    string switchURL    = telemetryServer + "/api/switch/recent";

    float elapsedSecs = 0.0f;           // Overall counter (currently not used)
    static float refreshSecs = 10.0f;    // How often to refresh (GET) the data from the telemetry server (seconds) - used later to do a refresh in the beginning, must be static cuz its initial
    float refreshTimer = refreshSecs;   // Timer used to trigger a data refresh (initialized to match so we immediately refresh) - used for refresh after

    // Use this for initialization
    void Start () {
        Debug.Log("Initializing ...");

        // Find the "Telemetry" Text canvas object and assign it to telemetryBox
        // The position, initial message, positioning, font, etc are all defined in the Unity Inspector
        if ((telemetryBox == null) && (transform.Find("Telemetry") != null)) {
            Transform child = transform.Find("Telemetry");
            telemetryBox = child.GetComponent<Text>();
            Debug.Log(telemetryBox.text);

            telemetryBox.text = "Requesting updated data ...";
            RefreshData();
        }

        // Find the "Alerts" Text canvas object and assign it to alertsBox
        if ((alertsBox == null) && (transform.Find("Alerts") != null))
        {
            Transform child = transform.Find("Alerts");
            alertsBox = child.GetComponent<Text>();
            Debug.Log(alertsBox.text);
        }
    }

    // Update is called once per frame
    void Update() {

        if ((telemetryBox == null) || (alertsBox == null)) { return; }  // too soon, not initialized yet

        // Track overall elapsed time
        elapsedSecs += Time.deltaTime;

        // Increment timer with time elapsed since the last update
        refreshTimer += Time.deltaTime; //time delta is basically framerate (how often update happens)

        if (refreshTimer > refreshSecs) {                    //refreshTimer is now how long its been and refreshSecs is still 10
            // Time to refresh the data, reset the timer and pull new suit data
            refreshTimer = 0; //reset the timer
            RefreshData(); //refresh the data

            // Update displayed telemetry data
            List<string> metrics = new List<string>();
            metrics.Add("Sub Pressure: " + telemetrydata.p_sub.ToString());
            metrics.Add("Env Temp: " + telemetrydata.t_sub.ToString());
            var telemetrymessage = String.Join("\n", metrics.ToArray());
            telemetryBox.text = telemetrymessage;

            // Check for and display any alerts
            List<string> alerts = new List<string>(); // A list of strings, we will join them with newlines at the end
            if (switchdata.sop_on)        { alerts.Add("Secondary Oxygen ACTIVE"); }
            if (switchdata.sspe)          { alerts.Add("Spacesuit Pressure EMERGENCY"); }
            if (switchdata.fan_error)     { alerts.Add("Fan FAILURE"); }
            if (switchdata.vent_error)    { alerts.Add("Vent ERROR"); }
            if (switchdata.vehicle_power) { alerts.Add("Vehicle Power PRESENT"); }
            if (switchdata.h2o_off)       { alerts.Add("H2O System OFFLINE"); }
            if (switchdata.o2_off)        { alerts.Add("O2 System OFFLINE"); }
            var alertmessage = String.Join("\n", alerts.ToArray());
            alertsBox.text = alertmessage;
        }
    }

    void RefreshData()
    {
        using (var webClient = new System.Net.WebClient()) //found online about how to do web requst in unity
        {
            var json = webClient.DownloadString(telemetryURL);
            //var json = "{\"p_sub\":2, \"t_sub\":75, \"v_fan\":20000, \"t_eva\":\"00:00:00\", \"p_o2\":850, \"rate_o2\":0.75, \"cap_battery\":15, \"p_h2o_g\":15, \"p_h2o_l\":15, \"p_sop\":850, \"rate_sop\":0.75 }";
            Debug.Log("TELEMETRY DATA: " + json);

            // Deserialize telemetry json into telemetrydata
            //telemetrydata = JsonUtility.FromJson<TelemetryData>(json);  // FromJson creates a completely new instance
            JsonUtility.FromJsonOverwrite(json, telemetrydata);           // updates objects in telemetrydata class with the data from json get

            json = webClient.DownloadString(switchURL); //redefines the json variable with the switch data now
            //json = "{ \"sop_on\" : false, \"sspe\" : false, \"fan_error\" : false, \"vent_error\" : true, \"vehicle_power\" : false, \"h2o_off\" : false, \"o2_off\" : false }";
            Debug.Log("SWITCH DATA: " + json);
            
            // Deserialize switch json into switchdata
            JsonUtility.FromJsonOverwrite(json, switchdata); //udpates the objects in switch class
        }
    }
}

