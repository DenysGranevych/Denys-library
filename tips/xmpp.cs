 //jaber
            //JabberClient jc = new JabberClient();
            //JID j = new JID("admin", "ec2-52-90-56-15.compute-1.amazonaws.com",null);
            //jc.User = j.User;
            //jc.Server = "ec2-52-90-56-15.compute-1.amazonaws.com";
            ////jc.Resource = "secondaryscreen";
            ////jc.Server = "52.90.56.15";
            ////jc.Resource = "ec2-52-90-56-15.compute-1.amazonaws.com";
            ////jc.Port = 5222;
            ////jc.Resource = "secondaryscreen";
            //jc.Password = "ahuzez90QA+90";
            ////jc.AutoRoster = true;
            //////jc.AutoStartTLS = TLS;
            //////jc.AutoPresence = initialPresence;
            ////jc.SSL = false;
            ////jc.KeepAlive = 10;
            ////jc.PlaintextAuth = true;
            ////jc.AutoStartTLS = false;
            ////jc.AutoStartCompression = false;
            //jc.AutoLogin = false;
            //jc.Connect();
            //jc.Login();

            //var test = jc.IsAuthenticated;
            ////jc.Register(j);

            ////jc.AutoLogin = false;
            //jc.Close(true);


            //ubiety

            //Xmpp.Settings.AuthenticationTypes = MechanismType.Plain;
            //Xmpp.Settings.Ssl = false;
            //Xmpp.Settings.Hostname = "ec2-52-90-56-15.compute-1.amazonaws.com";
            //Xmpp.Settings.Id = "admin@secondaryscreen";
            //Xmpp.Settings.Password = "ahuzez90QA+90";
            //Xmpp.Settings.Port = 5280;

            //Xmpp ubiety = new Xmpp();
            //ubiety.OnError += delegate
            //{
            //    Console.WriteLine("ds");
            //};
            //ubiety.Connect();
            //var res = Xmpp.Connected;


            //agsXMPP SDK

           // var xmpp = new XmppClientConnection("ejabberd@ip-172-31-5-147", 5222);

           // xmpp.UseStartTLS = false;
           // xmpp.UseSSL = false;
           // xmpp.AutoPresence = true;
           // xmpp.Port = 5222;
           // xmpp.Server = "ejabberd@ip-172-31-5-147";
           //// xmpp.ConnectServer = "ec2-52-90-56-15.compute-1.amazonaws.com";
           // xmpp.Username = "admin@secondaryscreen";
           // xmpp.Password = "ahuzez90QA+90";
            //xmpp.ConnectServer = "52.90.56.15";
            //xmpp.AutoPresence = true;
            //xmpp.AutoRoster = true;
            //xmpp.RegisterAccount = true;
            //xmpp.ConnectServer = "127.0.0.1";

            //xmpp.AutoResolveConnectServer = true;
            //xmpp.AutoPresence = true;

            //xmpp.Status = "available";
            //xmpp.Username = "newUser";
            //xmpp.Password = "password";

            //xmpp.KeepAlive = true;
            //xmpp.RegisterAccount = true;
            //agsXMPP.protocol.iq.register.Register reg = new agsXMPP.protocol.iq.register.Register("user", "password");
            //xmpp.Open();


            //xmpp.SocketConnectionType = SocketConnectionType.Direct;
            //xmpp.AutoRoster = true;
            //xmpp.AutoPresence = true;
            //xmpp.AutoResolveConnectServer = false;
            //xmpp.AutoAgents = false;
            //xmpp.RegisterAccount = true;
            //xmpp.UseCompression = false;
            //try
            //{
            //    //xmpp.OnLogin += new ObjectHandler(xmppCon_OnLogin);
            //    //xmpp.Open("admin", "ahuzez90QA+90");//admin@secondaryscreen
            //    xmpp.OnAuthError += loginFailed;
            //    xmpp.OnRegisterError += loginFailed;
            //    xmpp.OnRegistered += delegate(object o)
            //    {
            //        Console.WriteLine("sdf");
            //    };
            //    xmpp.OnLogin += delegate(object o)
            //    {
            //        xmpp.Send(new Message("test@jabber.org", MessageType.chat, "Hello, how are you?"));
            //    };

            //    //agsXMPP.protocol.sasl.MechanismType.PLAIN;
            //    xmpp.OnAuthError += loginFailed;

            //    //var auth = new agsXMPP.protocol.sasl.Auth();
            //    //auth.Attributes.Add("mechanism", "PLAIN");
            //    xmpp.Open("admin@secondaryscreen", "ahuzez90QA+90");//admin@secondaryscreen

            //    //xmpp.Open();
            //    //done.WaitOne();
            //    while (xmpp.XmppConnectionState == XmppConnectionState.Connecting)
            //    {
            //        //Thread.Sleep(1000);
            //    }
            //    //xmpp.Send(reg);
            //    xmpp.SendMyPresence();
            //    xmpp.RosterManager.AddRosterItem(new Jid("user@secondaryscreen"));
            //    //Presence p = new Presence(ShowType.chat, "Online");
            //    //p.Type = PresenceType.available;
            //    //xmpp.Send(p);
            //    //xmpp.SendMyPresence();
            //    var temp = xmpp.XmppConnectionState;
            //    var temp2 = xmpp.Authenticated;
            //    //xmpp.Send();
            //    xmpp.Close();
            //}
            ////xmpp.RegisterAccount
            //catch (Exception)
            //{

            //    throw;
            //}