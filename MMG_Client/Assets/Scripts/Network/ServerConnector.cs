using Newtonsoft.Json;
using Packet;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ServerConnector : SceneSingleton<ServerConnector>
{
    public enum ServerType { Main_Server, Chat_Server }

 
    private void Start()
    {
        PacketManager.Register();
    }

    public IEnumerator ConnectToMainServer()
    {
        bool isConnected = false;
        // TCP 연결 시도
        Debug.Log("서버 연결 시도 중...");

        Connect(
            () =>
            {
                isConnected = true;
                return new ClientSession();
            },
            ServerType.Main_Server
        );

        // 진짜 연결될 때까지 기다리기
        float timeout = 5f;
        float elapsed = 0f;
        while (!isConnected && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!isConnected)
        {
            Debug.LogError("서버 연결 실패!");
            // 에러 UI로 전환하거나 재시도 유도
            yield break;
        }

        Debug.Log("서버 연결 완료");

        // [1] 서버에 로그인 토큰 알려주기
        string loginToken = PlayerPrefs.GetString("jwt_token");
        if (loginToken != null)
        {
            C_LoginToken c_LoginToken = new C_LoginToken()
            {
                JwtToken = loginToken,
            };
            NetworkManager.Instance.Send_Login(c_LoginToken);
        }
        else
        {
            Debug.LogError("유저의 로그인 토큰이 없습니다. ");
        }
    }
    public void ConnctChatServer(Action action)
    {
        StartCoroutine(ConnectToChatServer(action));
    }
    public IEnumerator ConnectToChatServer(Action action)
    {
        bool isConnected = false;
        Debug.Log("채팅 서버 연결 시도 중...");

        Connect(
            () =>
            {
                isConnected = true;
                return new ChatSession();
            },
            ServerType.Chat_Server
        );
        float timeout = 5f;
        float elapsed = 0f;
        while (!isConnected && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!isConnected)
        {
            Debug.LogError("채팅 서버 연결 실패!");
            yield break;
        }

        Debug.Log("채팅 서버 연결 완료");
        action.Invoke();
    }
    public void Connect<TSession>(Func<TSession> sessionFactory, ServerType serverType)
    where TSession : Session
    {
        IPEndPoint endPoint = GetEndPoint(serverType);
        Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.RemoteEndPoint = endPoint;
        args.UserToken = sessionFactory;

        // 람다에서 serverType 캡처
        args.Completed += (s, e) => OnConnectCompleted(s, e, serverType);

        bool pending = socket.ConnectAsync(args);
        if (!pending)
            OnConnectCompleted(null, args, serverType);
    }
    //public void Connect(Func<ClientSession> sessionFactory)
    //{
    //    IPEndPoint endPoint = GetEndPoint();
    //    Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    //    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
    //    args.RemoteEndPoint = endPoint; 
    //    args.Completed += (s, e) => OnConnectCompleted(s, e, ServerType.Chat_Server);
    //    args.UserToken = sessionFactory;
    //    bool pending = socket.ConnectAsync(args);
    //    if (!pending)
    //        OnConnectCompleted(null, args, ServerType.Main_Server);
    //}

    private void OnConnectCompleted(object sender, SocketAsyncEventArgs args, ServerType serverType = ServerType.Main_Server)
    {
        if (args.SocketError == SocketError.Success)
        {
            if (serverType == ServerType.Main_Server)
            {
                ClientSession session = ((Func<ClientSession>)args.UserToken).Invoke();

                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
                SessionManager.Register(session);
                NetworkManager.Instance.GetClientSession(session);
            }
            else if (serverType == ServerType.Chat_Server)
            {
                ChatSession session = ((Func<ChatSession>)args.UserToken).Invoke();
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
                SessionManager.Register(session);
                NetworkManager.Instance.GetClientSession(session);
            }
        }
        else
        {
            Debug.LogError($"[Connect Failed] {args.SocketError}");
        }
    }


    //private void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
    //{
    //    if (args.SocketError == SocketError.Success)
    //    {
    //        ClientSession session = ((Func<ClientSession>)args.UserToken).Invoke();
    //        session.Start(args.ConnectSocket);
    //        session.OnConnected(args.RemoteEndPoint);
    //        SessionManager.Register(session);
    //        NetworkManager.Instance.GetClientSession(session);
    //    }
    //    else
    //    {
    //        Debug.LogError($"[Connect Failed] {args.SocketError}");
    //    }
    //}

    public static IPEndPoint GetEndPoint(ServerType type)
    {
        string host = Dns.GetHostName();
        IPAddress ipAddr = Dns.GetHostEntry(host)
            .AddressList
.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

        int portNumber = type == ServerType.Main_Server ? NetworkManager.MAIN_PORT_NUMBER : NetworkManager.CHAT_PORT_NUMBER;
        IPEndPoint endPoin = new IPEndPoint(ipAddr, portNumber);
        Debug.Log($"PortType : {type.ToString()}, PortNumber {portNumber}");
        Debug.Log($"서버 주소? ipAddr : {ipAddr}");
        return endPoin;
    }
}
