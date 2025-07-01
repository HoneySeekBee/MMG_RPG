using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ServerConnector : MonoBehaviour
{
    public static ServerConnector Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }

        Instance = this;
    }
    private void Start()
    {
        PacketManager.Register();
    }
    public IEnumerator ConnectToServer()
    {
        bool isConnected = false;
        // TCP 연결 시도
        Debug.Log("서버 연결 시도 중...");

        Connect(
               () =>
               {
                   isConnected = true;
                   return new ClientSession();
               });

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
        MapManager.Instance.EnterGameScene(GameManager.Instance.MapNumber);
    }
    public void Connect(Func<ClientSession> sessionFactory)
    {
        IPEndPoint endPoint = GetEndPoint();
        Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.RemoteEndPoint = endPoint;
        args.Completed += OnConnectCompleted;
        args.UserToken = sessionFactory;

        bool pending = socket.ConnectAsync(args);
        if (!pending)
            OnConnectCompleted(null, args);
    }
    private void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            ClientSession session = ((Func<ClientSession>)args.UserToken).Invoke();
            session.Start(args.ConnectSocket);
            session.OnConnected(args.RemoteEndPoint);
            SessionManager.Register(session);
            NetworkManager.Instance.GetClientSession(session);
        }
        else
        {
            Debug.LogError($"[Connect Failed] {args.SocketError}");
        }
    }
    public static IPEndPoint GetEndPoint()
    {
        string host = Dns.GetHostName();
        IPAddress ipAddr = Dns.GetHostEntry(host)
            .AddressList
.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

        IPEndPoint endPoin = new IPEndPoint(ipAddr, 7777);
        Debug.Log($"서버 주소? ipAddr : {ipAddr}");
        return endPoin;
    }
}
