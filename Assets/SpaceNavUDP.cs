using UnityEngine;
using System.Collections;
     
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
     
//This class is used to set up a UDP server to communicate with the trajectory server.  It sends a request for a trajectory and is given one as a response
public class SpaceNavUDP
{

		//Initialization variables
		private static int localPort;
		public int numReceived = 0;
		public string lastReceivedUDPPacket = "";
		private string IP;
		public int port;
		IPEndPoint remoteEndPoint;
		public UdpClient client;
		public Thread receiveThread;
            
		//Initialize the UDP server 
		public void init ()
		{
				//Listen on the local host at the selected port
				IP = "127.0.0.1"; 	//Local IP address
				port = 2000; 		//Selected local port
           
				//Set up a sender to the local server
				remoteEndPoint = new IPEndPoint (IPAddress.Parse (IP), port);
				client = new UdpClient ();

				//Get the game controller in order to act on sub-objects
				GameObject control = GameObject.FindGameObjectWithTag ("GameController");
				spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");

				string level = "";
				switch (ss.automate_level) {
				case pathMarker.SIMILAR:
						sendString ("init" + 0);
						break;
				case pathMarker.DISSIMILAR:
						sendString ("init" + 1);
						break;
				case pathMarker.STRAIGHT:
						sendString ("init" + 2);
						break;
				case pathMarker.NONE:
						sendString ("init" + 3);
						break;
				}
				receiveThread = new Thread (new ThreadStart (ReceiveData));
				receiveThread.IsBackground = true;
				receiveThread.Start ();
		}

		//Sends a given string the the remote server
		public void sendString (string message)
		{
				try {
						//Transslate data to UTF8-encoding
						byte[] data = Encoding.UTF8.GetBytes (message);
     
						//Send the message to the remote client
						client.Send (data, data.Length, remoteEndPoint);
				} catch (Exception err) { 
						//Catch any errors
						Debug.Log ("transmit catch found " + err.ToString ());
				}
		}

		//Sets up a listener to receive data transmissions back from the trajectory sserver
		private  void ReceiveData ()
		{
				while (true) {
						try {
								//Receieve a string from the remote host and put it into data
								IPEndPoint anyIP = new IPEndPoint (IPAddress.Any, 0);
								byte[] data = client.Receive (ref anyIP);
				
								//Read string with UTF8-encoding
								string text = Encoding.UTF8.GetString (data);
								numReceived++;

								//Store the received UDP packet
								lastReceivedUDPPacket = text;
						} catch (Exception err) {
								//Catch any errors
								Debug.LogError ("Receive exception found " + err.ToString ());
						}
				}
		} 
}
     