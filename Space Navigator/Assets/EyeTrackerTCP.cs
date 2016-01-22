using UnityEngine;
using System.Collections;

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class EyeTrackerTCP 
{

	private TcpClient m_client;
	private NetworkStream m_stream;
	private StreamWriter m_stream_write;
	private StreamReader m_stream_reader;
	private bool connectionReady = false;
	String host = "127.0.0.1";
	Int32 port = 4242;
	public float timeOut = 0f, timeOutLimit = 30f;
	
	//Some of the Commands for the Mirametrix S2 Eye Tracker
	public string send_counter_on = "<SET ID=\"ENABLE_SEND_COUNTER\" STATE=\"1\" />\r\n";
	public string send_pog_fix_on = "<SET ID=\"ENABLE_SEND_POG_FIX\" STATE=\"1\" />\r\n";
	public string send_data_on = "<SET ID=\"ENABLE_SEND_DATA\" STATE=\"1\" />\r\n";
	public string calibrate_show_on = "<SET ID=\"CALIBRATE_SHOW\" STATE=\"1\" />\r\n";
	public string calibrate_start_on = "<SET ID=\"CALIBRATE_START\" STATE=\"1\" />\r\n";
	public string send_counter_off = "<SET ID=\"ENABLE_SEND_COUNTER\" STATE=\"0\" />\r\n";
	public string send_pog_fix_off = "<SET ID=\"ENABLE_SEND_POG_FIX\" STATE=\"0\" />\r\n";
	public string send_data_off = "<SET ID=\"ENABLE_SEND_DATA\" STATE=\"0\" />\r\n";
	public string calibrate_show_off = "<SET ID=\"CALIBRATE_SHOW\" STATE=\"0\" />\r\n";
	public string calibrate_start_off = "<SET ID=\"CALIBRATE_START\" STATE=\"0\" />\r\n";
	public string get_screen_size = "<GET ID=\"SCREEN_SIZE\" /> \r\n";
	public string enable_send_time_tick = "<SET ID=\"ENABLE_SEND_TIME_TICK\" STATE=\"1\" />\r\n";
	public string time_tick_frequency = "<GET ID=\"TIME_TICK_FREQUENCY\" />\r\n";
	//Threading used to keep gathering eye tracking information
	public Thread receiveThread;
	public string lastReceivedThread = "";
	private bool connectionOn = true;
	public float screen_height, screen_width, time_tick_f;
	public string id = "";
	public int gamelevel = 0;
	public static float timer;
	public static float seconds;
	//Call to initialize eye tracker connection
	//tracker.exe must be open to work
	public void init()
	{
		//
		//Connection time out loop
		while (!connectionReady && timeOut < timeOutLimit)
		{
			setUpSocket ();
			timeOut += Time.deltaTime;
		}
		if (timeOut > timeOutLimit) 
		{
			Debug.Log ("Time out has occurred, please start the server.");
		} else {
			//Initialize data stream
			writeStream (send_counter_on);
			writeStream (send_pog_fix_on);
			writeStream (get_screen_size);
		}
		//Initialize thread
		receiveThread = new Thread (new ThreadStart (accumulateData));
		receiveThread.IsBackground = true;
		receiveThread.Start ();
	}

	
	public void calibrate()
	{
		//Connection time out loop
		while (!connectionReady && timeOut < timeOutLimit)
		{
			setUpSocket ();
			timeOut += Time.deltaTime;
		}
		if (timeOut > timeOutLimit) 
		{
			Debug.Log ("Time out has occurred, please start the server.");
		} else {
			Debug.Log ("Connection");
			//Initialize data stream
			writeStream(calibrate_show_on);
			writeStream (calibrate_start_on);
			int timeStart = System.DateTime.Now.Second;
			int timeEnd = 30;
			int timeTick = 0;
			while (timeTick < timeEnd)
			{
				int now = System.DateTime.Now.Second;
				if (timeStart <= now)
				{
					timeTick = now - timeStart;
				} else if (timeStart > now)
				{
					timeTick = (now+60)-timeStart;
				}
			}
		}
		//For some reason the eye tracker doesn't close out of the calibration if we write to it here.
		//So this closes and reopen the socket to close the calibration window within the tablet enviornment.
		//Otherwise you have to use a janky hack to do it in Windows 8.1
		closeStreamNotThread();
		//Connection time out loop
		while (!connectionReady && timeOut < timeOutLimit)
		{
			setUpSocket ();
			timeOut += Time.deltaTime;
		}
		writeStream (send_data_off);
		writeStream (calibrate_show_off);
		closeStreamNotThread();
	}
	
	//accumulates the eye tracking fixed point of gaze data and then appends it to the eye tracker data file
	private void accumulateData()
	{
		string str = "";
		bool scrn_known = false;
		int count = 0;
		while (connectionOn)
		{
			str = readStream ();
			// Used to determine screen size and that the string is full length.
			if (str.Contains("SCREEN_SIZE") && (!scrn_known) && str.Contains ("/>"))
			{
				//Split string for screen size
				string[] find_query = str.Split('\"');
				//Refer to Mirametrix S2 Eye-tracker API documentation for positions of height and width in the XML string
				screen_height = float.Parse (find_query[5]);
				Debug.Log (screen_height);
				screen_width = float.Parse (find_query[3]);
				Debug.Log (screen_width);
				scrn_known = true;
				if (scrn_known)
				{
					//Wait to send data until now so that the screen size query is not overloaded with eye tracker information
					writeStream (send_data_on);
				}
			} else if (str.Length > 19 && !scrn_known) {
				//Re-get screen-size if it has not been detected already.
				writeStream (get_screen_size);
			}
			//Once strings are full grown, process them for data.
			if (str.Length > 2 && str.Substring(str.Length-2, 2) == "/>")
			{
				// process string (print and clear)
				string txy = processStringForXY(str);
				lastReceivedThread += txy;
				str = "";
				count++;
				if (count > 300)
				{
					string fileName = "current_level.txt";
					if (File.Exists (fileName))
					{
						id = read_id(fileName);
					}
					File.AppendAllText(id + "eyeTrackerGD" +gamelevel + ".txt", lastReceivedThread);
					count = 0;
					lastReceivedThread = "";
				}
			}
		}
	}
	
	public string processStringForXY(string str)
	{
			if (str.Contains ("FPOGX") && str.Contains ("FPOGY"))
			{
				string[] data_split = str.Split ('\"');
				str = "";
				//Local system time in seconds
				str = (System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second) + "." + System.DateTime.Now.Millisecond.ToString () + ",";
				//FPOGX
				if (float.Parse (data_split[3]) == 0 || float.Parse (data_split[5]) == 0)
				{
					return "";
				}
				str += (float.Parse (data_split[3])*screen_width).ToString ();
				str += ",";
				//FPOGY
				str += (float.Parse (data_split[5])*screen_height).ToString ();
				str += ",";
			}
			return str;
	}
	
	public bool setUpSocket() {
		//Attemtps to connect to the TCP host.
		try {
			m_client = new TcpClient(host, port);
			m_stream = m_client.GetStream ();
			m_stream_write = new StreamWriter(m_stream);
			m_stream_reader = new StreamReader(m_stream);
			connectionReady = true;
			Debug.Log ("Connection: " + connectionReady);
		} catch (Exception e)
		{
			Debug.Log("Socket error:" + e);
		}
		return connectionReady;
	}
	//Writes to the TCP host given a string
	public void writeStream(string s_toWrite)
	{
		if (!connectionReady) { return; }
		String S_toWrite = s_toWrite;
		m_stream_write.Write(S_toWrite);
		m_stream_write.Flush ();
	}
	//Reads a line from the TCP host
	public string readStream() {
		if (!connectionReady) { return ""; }
		try {
			return m_stream_reader.ReadLine ();
		} catch (Exception e) {
			return "Stream Exception:" + e;
		}
	}
	//Closes the TCP stream, turns data sending off first.
	public void closeStream() {
		if (!connectionReady) { return; }
		writeStream (send_data_off);
		writeStream (calibrate_show_off);
		receiveThread.Abort();
		m_stream_write.Close();
		m_stream_reader.Close ();
		m_stream.Close();
		connectionReady = false;
		Debug.Log ("Connection: " + connectionReady);
	}
	//Closes the TCP stream, turns data sending off first.
	public void closeStreamNotThread() {
		if (!connectionReady) { return; }
		writeStream (send_data_off);
		writeStream (calibrate_show_off);
		m_stream_write.Close();
		m_stream_reader.Close ();
		m_stream.Close();
		connectionReady = false;
		Debug.Log ("Connection: " + connectionReady);
	}
	public string read_id(string fileName)
	{
		//fileName = "Level_1.txt";
		//Debug.Log ("Loading: " + fileName);
		string line;
		System.IO.StreamReader file = new System.IO.StreamReader (fileName);
		//Reads past game level
		line = file.ReadLine();
		//Reads past new line
		line = file.ReadLine();
		//Reads unique_id
		line = file.ReadLine();
		string[] chunk = line.Split (new char[]{':'});
		string id = chunk[1];
		return id;
	}
}
