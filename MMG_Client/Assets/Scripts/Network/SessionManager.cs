using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionManager
{
    public static ClientSession MySession { get; private set; }

    public static void Register(ClientSession session)
    {
        MySession = session;
    }

    public static void Clear()
    {
        MySession = null;
    }
}
