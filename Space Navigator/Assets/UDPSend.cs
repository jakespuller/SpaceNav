using UnityEngine;
using System.Collections;
     
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
     
public class UDPSend
{
		private static int localPort;
		
		// infos
		public string lastReceivedUDPPacket = "";
		public string allReceivedUDPPackets = ""; // clean up this from time to time!

		// prefs
		private string IP;  // define in init
		public int port;  // define in init
       
		// "connection" things
		IPEndPoint remoteEndPoint;
		public UdpClient client;

		// receiving Thread
		public Thread receiveThread;
            
		// init
		public void init ()
		{
				// define
				IP = "127.0.0.1"; 	//Local IP address
				port = 2000; 		//Selected local port
           
				//Set up a sender to the local server
				remoteEndPoint = new IPEndPoint (IPAddress.Parse (IP), port);
				client = new UdpClient (port);
		        
				//Set up a receiver to listen for response
				receiveThread = new Thread (new ThreadStart (ReceiveData));
				receiveThread.IsBackground = true;
				receiveThread.Start ();
		}

		// sendData
		public void sendString (string message)
		{
				try {
						// Send data with UTF8-encoding
						byte[] data = Encoding.UTF8.GetBytes (message);
     
						// Send the message to the remote client
						client.Send (data, data.Length, remoteEndPoint);
				} catch (Exception err) {
						Debug.LogError (err.ToString ());
				}
		}

		// Receive thread
		private  void ReceiveData ()
		{
				while (true) {
						try {
								// Bytes empfangen.
								IPEndPoint anyIP = new IPEndPoint (IPAddress.Any, 0);
								//if (client.Active)
								//		Debug.Log ("boom");	
								byte[] data = client.Receive (ref anyIP);
				
								// Read string with UTF8-encoding
								string text = Encoding.UTF8.GetString (data);
				
								// Parrot back the text we receive
								Debug.Log (">> " + text);
				
								// latest UDPpacket
								lastReceivedUDPPacket = text;
				
								// ....
								allReceivedUDPPackets = allReceivedUDPPackets + text;
						} catch (Exception err) {
								//Debug.LogError (err.ToString ());
						}
				}
		} 
}
     